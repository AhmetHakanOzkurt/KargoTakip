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
        private readonly KargoTakip.ServiceDefaults.YerelZaman _yerelZaman;

        public ReportController(
            KargoTakipDbContext context,
            KargoTakip.ServiceDefaults.YerelZaman yerelZaman)
        {
            _context = context;
            _yerelZaman = yerelZaman;
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
                        .Count(s => s.CurrentStatus == ShipmentStatus.TeslimEdildi),
                    DevamEden = b.Shipments
                        .Count(s => s.CurrentStatus != ShipmentStatus.TeslimEdildi),
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
            [FromQuery] int? subeId,
            [FromQuery] int? sayfa,
            [FromQuery] int? sayfaBoyutu)
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

            // Tarih araligi zorunlu degil; sayfalama olmadan tum tablo donuyordu.
            var aktifSayfa = sayfa.GetValueOrDefault(1) < 1 ? 1 : sayfa.GetValueOrDefault(1);
            var boyut = sayfaBoyutu.GetValueOrDefault(100);
            boyut = boyut < 1 ? 100 : (boyut > 500 ? 500 : boyut);

            var toplamKayit = await query.CountAsync();

            var report = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((aktifSayfa - 1) * boyut)
                .Take(boyut)
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
                .ToListAsync();

            return Ok(new
            {
                toplamKayit,
                sayfa = aktifSayfa,
                sayfaBoyutu = boyut,
                kayitlar = report
            });
        }

        // Günlük özet
        [HttpGet("daily")]
        public async Task<IActionResult> GetDailyReport()
        {
            // Gun siniri yerel saat diliminde hesaplanir; UtcNow.Date ile
            // gunun ilk 3 saati onceki gune dusuyordu.
            var bugun = _yerelZaman.BugunBaslangicUtc();
            var yarin = _yerelZaman.BugunBitisUtc();

            var bugunOlusturulan = await Kargolar()
                .CountAsync(s => s.CreatedAt >= bugun && s.CreatedAt < yarin);

            var bugunTeslimEdilen = await Kargolar()
                .CountAsync(s => s.CurrentStatus == ShipmentStatus.TeslimEdildi
                    && s.UpdatedAt >= bugun && s.UpdatedAt < yarin);

            var bugunYolda = await Kargolar()
                .CountAsync(s => s.CurrentStatus == ShipmentStatus.Yolda
                    && s.UpdatedAt >= bugun && s.UpdatedAt < yarin);

            var aktifAracSayisi = await Araclar()
                .CountAsync(v => !v.IsAvailable);

            return Ok(new
            {
                tarih = _yerelZaman.BugunYerelTarih(),
                saatDilimi = _yerelZaman.SaatDilimiAdi,
                bugunOlusturulan,
                bugunTeslimEdilen,
                bugunYolda,
                aktifAracSayisi
            });
        }
    }
}