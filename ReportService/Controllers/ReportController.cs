using KargoTakip.Infrastructure.Data;
using KargoTakip.Infrastructure.Models;
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

        private string? CurrentRole =>
            User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        private int CurrentBranchId
        {
            get
            {
                int.TryParse(User.FindFirst("branchId")?.Value, out int branchId);
                return branchId;
            }
        }

        private bool AdminMi => CurrentRole == "Admin";

        // Admin disindaki kullanicilar yalnizca kendi subelerinin verisini gorur.
        private IQueryable<Shipment> Kargolar() =>
            AdminMi
                ? _context.Shipments
                : _context.Shipments.Where(s => s.BranchId == CurrentBranchId);

        private IQueryable<Vehicle> Araclar() =>
            AdminMi
                ? _context.Vehicles
                : _context.Vehicles.Where(v => v.BranchId == CurrentBranchId);

        // Genel özet raporu
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var toplamKargo = await Kargolar().CountAsync();

            var durumDagilimi = await Kargolar()
                .GroupBy(s => s.CurrentStatus)
                .Select(g => new
                {
                    Durum = g.Key,
                    Adet = g.Count()
                })
                .ToListAsync();

            var oncelikDagilimi = await Kargolar()
                .GroupBy(s => s.Priority)
                .Select(g => new
                {
                    Oncelik = g.Key,
                    Adet = g.Count()
                })
                .ToListAsync();

            // Bos kumede SUM NULL doner; decimal? ile guvenli sekilde 0'a dusurulur.
            var toplamAgirlik = await Kargolar()
                .SumAsync(s => (decimal?)s.Weight) ?? 0m;

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
            var subeSorgusu = AdminMi
                ? _context.Branches
                : _context.Branches.Where(b => b.Id == CurrentBranchId);

            var report = await subeSorgusu
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
            var report = await Araclar()
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
            var query = Kargolar()
                .Include(s => s.Branch)
                .Include(s => s.ReceiverCity)
                .AsQueryable();

            // Admin olmayan kullanici baska subeyi sorgulayamaz
            if (!AdminMi && subeId.HasValue && subeId.Value != CurrentBranchId)
                return Forbid();

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

            var bugunOlusturulan = await Kargolar()
                .CountAsync(s => s.CreatedAt >= bugun && s.CreatedAt < yarin);

            var bugunTeslimEdilen = await Kargolar()
                .CountAsync(s => s.CurrentStatus == "Teslim Edildi"
                    && s.UpdatedAt >= bugun && s.UpdatedAt < yarin);

            var bugunYolda = await Kargolar()
                .CountAsync(s => s.CurrentStatus == "Yolda"
                    && s.UpdatedAt >= bugun && s.UpdatedAt < yarin);

            var aktifAracSayisi = await Araclar()
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