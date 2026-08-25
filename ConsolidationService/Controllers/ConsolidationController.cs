using ConsolidationService.Services;
using KargoTakip.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConsolidationService.Controllers
{
    [ApiController]
    [Route("api/consolidation")]
    [Authorize]
    public class ConsolidationController : ControllerBase
    {
        private readonly KargoTakipDbContext _context;
        private readonly ConsolidationEngine _engine;

        public ConsolidationController(
            KargoTakipDbContext context,
            ConsolidationEngine engine)
        {
            _context = context;
            _engine = engine;
        }

        // Manuel tetikleme — test ve sunum için.
        // Tum subeleri tarayan agir bir islem; her kullanici tetikleyememeli.
        [HttpPost("run")]
        [Authorize(Roles = "Admin,BranchManager")]
        public async Task<IActionResult> RunNow()
        {
            await _engine.RunAsync();
            return Ok(new { message = "Konsolidasyon motoru çalıştırıldı." });
        }

        // Tüm planları listele
        [HttpGet("plans")]
        public async Task<IActionResult> GetPlans(
            [FromQuery] int? sayfa, [FromQuery] int? sayfaBoyutu)
        {
            // Diger listeleme uclarina sayfalama eklenirken burasi atlanmisti.
            var aktifSayfa = sayfa.GetValueOrDefault(1) < 1 ? 1 : sayfa.GetValueOrDefault(1);
            var boyut = sayfaBoyutu.GetValueOrDefault(50);
            boyut = boyut < 1 ? 50 : (boyut > 200 ? 200 : boyut);

            var toplamKayit = await _context.ConsolidationPlans.CountAsync();

            var plans = await _context.ConsolidationPlans
                .Include(p => p.Vehicle)
                .Include(p => p.OriginBranch)
                .Include(p => p.DestinationCity)
                .Include(p => p.Items)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((aktifSayfa - 1) * boyut)
                .Take(boyut)
                .Select(p => new
                {
                    p.Id,
                    p.Status,
                    Arac = p.Vehicle.PlateNumber,
                    CikisSube = p.OriginBranch.Name,
                    HedefSehir = p.DestinationCity.Name,
                    p.TotalCapacity,
                    p.UsedCapacity,
                    DolulukOrani = p.OccupancyRate,
                    TahminiYakitTasarrufu = p.EstimatedFuelSaving,
                    KargoSayisi = p.Items.Count,
                    p.PlannedDepartureAt,
                    p.ActualDepartureAt,
                    p.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                toplamKayit,
                sayfa = aktifSayfa,
                sayfaBoyutu = boyut,
                kayitlar = plans
            });
        }

        // Plan detayı
        [HttpGet("plans/{id}")]
        public async Task<IActionResult> GetPlan(int id)
        {
            var plan = await _context.ConsolidationPlans
                .Include(p => p.Vehicle)
                .Include(p => p.OriginBranch)
                .Include(p => p.DestinationCity)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Shipment)
                        .ThenInclude(s => s.ReceiverCity)
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Status,
                    Arac = p.Vehicle.PlateNumber,
                    CikisSube = p.OriginBranch.Name,
                    HedefSehir = p.DestinationCity.Name,
                    p.TotalCapacity,
                    p.UsedCapacity,
                    DolulukOrani = p.OccupancyRate,
                    TahminiYakitTasarrufu = p.EstimatedFuelSaving,
                    p.PlannedDepartureAt,
                    p.ActualDepartureAt,
                    p.CreatedAt,
                    Kargolar = p.Items.Select(i => new
                    {
                        i.ShipmentId,
                        i.Shipment.TrackingCode,
                        HedefSehir = i.Shipment.ReceiverCity.Name,
                        i.Shipment.CurrentStatus,
                        i.AddedReason
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (plan == null)
                return NotFound(new { message = "Plan bulunamadı." });

            return Ok(plan);
        }

        // Tasarruf özeti
        [HttpGet("savings")]
        public async Task<IActionResult> GetSavings()
        {
            var plans = await _context.ConsolidationPlans.ToListAsync();

            var toplamTasarruf = plans.Sum(p => p.EstimatedFuelSaving);
            var ortalamaDoluluk = plans.Any()
                ? plans.Average(p => p.OccupancyRate) : 0;
            var toplamSefer = plans.Count;
            var toplamKargo = await _context.ConsolidationPlanItems.CountAsync();

            return Ok(new
            {
                toplamSefer,
                toplamKargo,
                toplamTahminiTasarruf = Math.Round(toplamTasarruf, 2),
                ortalamaDolulukOrani = Math.Round(ortalamaDoluluk, 1)
            });
        }
    }
}