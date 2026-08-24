using KargoTakip.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly KargoTakipDbContext _context;

        public NotificationController(KargoTakipDbContext context)
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

        // Kullanici yalnizca kendi subesinin bildirimlerine erisebilir.
        private bool SubeyeErisebilir(int branchId) =>
            CurrentRole == "Admin" || branchId == CurrentBranchId;

        // Şubeye ait bildirimleri listele
        [HttpGet("{branchId}")]
        public async Task<IActionResult> GetByBranch(
            int branchId,
            [FromQuery] int? sayfa,
            [FromQuery] int? sayfaBoyutu)
        {
            if (!SubeyeErisebilir(branchId))
                return Forbid();

            // Onceden tum bildirimler tek seferde cekiliyordu.
            var aktifSayfa = sayfa.GetValueOrDefault(1) < 1 ? 1 : sayfa.GetValueOrDefault(1);
            var boyut = sayfaBoyutu.GetValueOrDefault(50);
            boyut = boyut < 1 ? 50 : (boyut > 200 ? 200 : boyut);

            var sorgu = _context.Notifications.Where(n => n.BranchId == branchId);
            var toplamKayit = await sorgu.CountAsync();

            var notifications = await sorgu
                .OrderByDescending(n => n.CreatedAt)
                .Skip((aktifSayfa - 1) * boyut)
                .Take(boyut)
                .Select(n => new
                {
                    n.Id,
                    n.Message,
                    n.IsRead,
                    n.CreatedAt,
                    ShipmentId = n.ShipmentId,
                    TransferRequestId = n.TransferRequestId
                })
                .ToListAsync();

            return Ok(new
            {
                toplamKayit,
                sayfa = aktifSayfa,
                sayfaBoyutu = boyut,
                kayitlar = notifications
            });
        }

        // Bildirimi okundu olarak işaretle
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return NotFound(new { message = "Bildirim bulunamadı." });

            if (!SubeyeErisebilir(notification.BranchId))
                return Forbid();

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Bildirim okundu olarak işaretlendi." });
        }

        // Okunmamış bildirim sayısı
        [HttpGet("{branchId}/unread-count")]
        public async Task<IActionResult> GetUnreadCount(int branchId)
        {
            if (!SubeyeErisebilir(branchId))
                return Forbid();

            var count = await _context.Notifications
                .CountAsync(n => n.BranchId == branchId && !n.IsRead);

            return Ok(new { branchId, unreadCount = count });
        }
    }
}