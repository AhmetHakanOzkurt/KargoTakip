using KargoTakip.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ReportService.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly KargoTakipDbContext _context;

        public ReportController(KargoTakipDbContext context)
        {
            _context = context;
        }

        // Genel özet raporu
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var toplamKargo = await _context.Shipments.CountAsync();

            var durumDagilimi = await _context.Shipments
                .GroupBy(s => s.CurrentStatus)
                .Select(g => new
                {
                    Durum = g.Key,
                    Adet = g.Count()
                })
                .ToListAsync();

            var oncelikDagilimi = await _context.Shipments
                .GroupBy(s => s.Priority)
                .Select(g => new
                {
                    Oncelik = g.Key,
                    Adet = g.Count()
                })
                .ToListAsync();

            var toplamAgirlik = await _context.Shipments
                .SumAsync(s => s.Weight);

            return Ok(new
            {
                toplamKargo,
                toplamAgirlik = Math.Round(toplamAgirlik, 2),
                durumDagilimi,
                oncelikDagilimi
            });
        }

        // Şube bazlı rapor
        [HttpGet("branches")]
        public async Task<IActionResult> GetBranchReport()
        {
            var report = await _context.Branches
                .Select(b => new
                {
                    SubeId = b.Id,
                    SubeAdi = b.Name,
                    ToplamKargo = b.Shipments.Count(),
                    TeslimEdilen = b.Shipments
                        .Count(s => s.CurrentStatus == "Teslim Edildi"),
                    DevamEden = b.Shipments
                        .Count(s => s.CurrentStatus != "Teslim Edildi"),
                    ToplamAgirlik = Math.Round(
                        b.Shipments.Sum(s => s.Weight), 2),
                    ToplamArac = b.Vehicles.Count(),
                    MüsaitArac = b.Vehicles
                        .Count(v => v.IsAvailable)
                })
                .ToListAsync();

            return Ok(report);
        }

        // Araç bazlı rapor
        [HttpGet("vehicles")]
        public async Task<IActionResult> GetVehicleReport()
        {
            var report = await _context.Vehicles
                .Include(v => v.VehicleType)
                .Include(v => v.Branch)
                .Select(v => new
                {
                    AracId = v.Id,
                    Plaka = v.PlateNumber,
                    Tip = v.VehicleType.Name,
                    Sube = v.Branch.Name,
                    Kapasite = v.Capacity,
                    MevcutYuk = v.CurrentLoad,
                    DolulukOrani = v.Capacity > 0
                        ? (int)Math.Round(
                            (double)v.CurrentLoad / v.Capacity * 100)
                        : 0,
                    MüsaitMi = v.IsAvailable,
                    TasinanKargo = _context.Shipments
                        .Count(s => s.AssignedVehicleId == v.Id)
                })
                .ToListAsync();

            return Ok(report);
        }

        // Tarih aralığına göre kargo raporu
        [HttpGet("shipments")]
        public async Task<IActionResult> GetShipmentReport(
            [FromQuery] DateTime? baslangic,
            [FromQuery] DateTime? bitis,
            [FromQuery] int? subeId)
        {
            var query = _context.Shipments
                .Include(s => s.Branch)
                .Include(s => s.ReceiverCity)
                .AsQueryable();

            if (baslangic.HasValue)
                query = query.Where(s => s.CreatedAt >= baslangic.Value);

            if (bitis.HasValue)
                query = query.Where(s => s.CreatedAt <= bitis.Value);

            if (subeId.HasValue)
                query = query.Where(s => s.BranchId == subeId.Value);

            var report = await query
                .Select(s => new
                {
                    s.Id,
                    s.TrackingCode,
                    s.SenderName,
                    s.ReceiverName,
                    Sube = s.Branch.Name,
                    HedefSehir = s.ReceiverCity.Name,
                    s.Weight,
                    s.Priority,
                    s.CurrentStatus,
                    s.CreatedAt,
                    s.UpdatedAt
                })
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return Ok(new
            {
                toplamKayit = report.Count,
                kayitlar = report
            });
        }

        // Günlük özet
        [HttpGet("daily")]
        public async Task<IActionResult> GetDailyReport()
        {
            var bugun = DateTime.UtcNow.Date;
            var yarin = bugun.AddDays(1);

            var bugunOlusturulan = await _context.Shipments
                .CountAsync(s => s.CreatedAt >= bugun && s.CreatedAt < yarin);

            var bugunTeslimEdilen = await _context.Shipments
                .CountAsync(s => s.CurrentStatus == "Teslim Edildi"
                    && s.UpdatedAt >= bugun && s.UpdatedAt < yarin);

            var bugunYolda = await _context.Shipments
                .CountAsync(s => s.CurrentStatus == "Yolda"
                    && s.UpdatedAt >= bugun && s.UpdatedAt < yarin);

            var aktifAracSayisi = await _context.Vehicles
                .CountAsync(v => !v.IsAvailable);

            return Ok(new
            {
                tarih = bugun.ToString("yyyy-MM-dd"),
                bugunOlusturulan,
                bugunTeslimEdilen,
                bugunYolda,
                aktifAracSayisi
            });
        }
    }
}