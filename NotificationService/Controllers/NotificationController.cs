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
        public async Task<IActionResult> GetByBranch(int branchId)
        {
            if (!SubeyeErisebilir(branchId))
                return Forbid();

            var notifications = await _context.Notifications
                .Where(n => n.BranchId == branchId)
                .OrderByDescending(n => n.CreatedAt)
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

            return Ok(notifications);
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