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

        // Şubeye ait bildirimleri listele
        [HttpGet("{branchId}")]
        public async Task<IActionResult> GetByBranch(int branchId)
        {
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

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Bildirim okundu olarak işaretlendi." });
        }

        // Okunmamış bildirim sayısı
        [HttpGet("{branchId}/unread-count")]
        public async Task<IActionResult> GetUnreadCount(int branchId)
        {
            var count = await _context.Notifications
                .CountAsync(n => n.BranchId == branchId && !n.IsRead);

            return Ok(new { branchId, unreadCount = count });
        }
    }
}