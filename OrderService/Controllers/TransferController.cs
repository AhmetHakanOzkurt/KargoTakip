using KargoTakip.Infrastructure.Data;
using KargoTakip.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/transfers")]
    [Authorize]
    public class TransferController : ControllerBase
    {
        private readonly KargoTakipDbContext _context;
        private readonly OrderService.Messaging.RabbitMqProducer _producer;
        private readonly ILogger<TransferController> _logger;

        public TransferController(
            KargoTakipDbContext context,
            OrderService.Messaging.RabbitMqProducer producer,
            ILogger<TransferController> logger)
        {
            _context = context;
            _producer = producer;
            _logger = logger;
        }

        // Transfer talebi oluştur
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateTransferRequest request)
        {
            var branchIdClaim = User.FindFirst("branchId")?.Value;
            int.TryParse(branchIdClaim, out int userBranchId);

            // Talep eden şube ile token'daki şube eşleşmeli
            if (request.RequesterBranchId != userBranchId &&
                User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value != "Admin")
                return Forbid();

            // Hedef şube var mı
            var targetBranch = await _context.Branches.FindAsync(request.TargetBranchId);
            if (targetBranch == null)
                return BadRequest(new { message = "Hedef şube bulunamadı." });

            // Kargolar bu şubeye ait mi kontrol et
            var shipmentIds = request.ShipmentIds.Distinct().ToList();
            var shipments = await _context.Shipments
                .Where(s => shipmentIds.Contains(s.Id) &&
                            s.BranchId == request.RequesterBranchId &&
                            s.CurrentStatus == "Hazırlanıyor")
                .ToListAsync();

            if (shipments.Count != shipmentIds.Count)
                return BadRequest(new
                {
                    message = "Bazı kargolar bu şubeye ait değil veya transfer edilemez durumda."
                });

            var userIdClaim = User.FindFirst("userId")?.Value;
            int.TryParse(userIdClaim, out int userId);

            var transferRequest = new TransferRequest
            {
                RequesterBranchId = request.RequesterBranchId,
                TargetBranchId = request.TargetBranchId,
                Status = "Bekliyor",
                Note = request.Note,
                RequestedByUserId = userId,
                RequestedAt = DateTime.UtcNow
            };

            _context.TransferRequests.Add(transferRequest);
            await _context.SaveChangesAsync();

            // Transfer kalemleri ekle
            var items = shipmentIds.Select(sid => new TransferRequestItem
            {
                TransferRequestId = transferRequest.Id,
                ShipmentId = sid
            }).ToList();

            _context.TransferRequestItems.AddRange(items);
            await _context.SaveChangesAsync();

            // Hedef şubeye bildirim gönder
            var notification = new Notification
            {
                TransferRequestId = transferRequest.Id,
                BranchId = request.TargetBranchId,
                Message = $"{targetBranch.Name} şubesinden {shipmentIds.Count} kargo için transfer talebi geldi.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // RabbitMQ'ya event yayınla
            await _producer.PublishAsync("transfer_talebi_olusturuldu", new
            {
                TransferRequestId = transferRequest.Id,
                RequesterBranchId = request.RequesterBranchId,
                TargetBranchId = request.TargetBranchId,
                KargoSayisi = shipmentIds.Count
            });

            _logger.LogInformation(
                "Transfer talebi oluşturuldu: {Id}, {Count} kargo",
                transferRequest.Id, shipmentIds.Count);

            return CreatedAtAction(nameof(GetById),
                new { id = transferRequest.Id }, new
                {
                    transferRequest.Id,
                    transferRequest.Status,
                    kargoSayisi = shipmentIds.Count,
                    message = "Transfer talebi oluşturuldu."
                });
        }

        // Transfer talebini getir
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var branchIdClaim = User.FindFirst("branchId")?.Value;
            int.TryParse(branchIdClaim, out int userBranchId);
            var role = User.FindFirst(
                System.Security.Claims.ClaimTypes.Role)?.Value;

            var transfer = await _context.TransferRequests
                .Include(t => t.RequesterBranch)
                .Include(t => t.TargetBranch)
                .Include(t => t.RequestedByUser)
                .Include(t => t.RespondedByUser)
                .Include(t => t.Items)
                    .ThenInclude(i => i.Shipment)
                .Where(t => t.Id == id)
                .Where(t => role == "Admin" ||
                            t.RequesterBranchId == userBranchId ||
                            t.TargetBranchId == userBranchId)
                .Select(t => new
                {
                    t.Id,
                    t.Status,
                    t.Note,
                    t.ScheduledAt,
                    t.RequestedAt,
                    t.RespondedAt,
                    TalepEdenSube = t.RequesterBranch.Name,
                    HedefSube = t.TargetBranch.Name,
                    TalepEden = t.RequestedByUser.FullName,
                    Yanitleyen = t.RespondedByUser != null
                        ? t.RespondedByUser.FullName : null,
                    Kargolar = t.Items.Select(i => new
                    {
                        i.ShipmentId,
                        i.Shipment.TrackingCode,
                        i.Shipment.CurrentStatus
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (transfer == null)
                return NotFound(new { message = "Transfer talebi bulunamadı." });

            return Ok(transfer);
        }

        // Şubeye gelen transfer taleplerini listele
        [HttpGet("incoming")]
        public async Task<IActionResult> GetIncoming()
        {
            var branchIdClaim = User.FindFirst("branchId")?.Value;
            int.TryParse(branchIdClaim, out int userBranchId);
            var role = User.FindFirst(
                System.Security.Claims.ClaimTypes.Role)?.Value;

            var query = _context.TransferRequests
                .Include(t => t.RequesterBranch)
                .Include(t => t.Items)
                .AsQueryable();

            if (role != "Admin")
                query = query.Where(t => t.TargetBranchId == userBranchId);

            var transfers = await query
                .Select(t => new
                {
                    t.Id,
                    t.Status,
                    TalepEdenSube = t.RequesterBranch.Name,
                    KargoSayisi = t.Items.Count,
                    t.RequestedAt,
                    t.ScheduledAt
                })
                .OrderByDescending(t => t.RequestedAt)
                .ToListAsync();

            return Ok(transfers);
        }

        // Şubenin gönderdiği transfer taleplerini listele
        [HttpGet("outgoing")]
        public async Task<IActionResult> GetOutgoing()
        {
            var branchIdClaim = User.FindFirst("branchId")?.Value;
            int.TryParse(branchIdClaim, out int userBranchId);
            var role = User.FindFirst(
                System.Security.Claims.ClaimTypes.Role)?.Value;

            var query = _context.TransferRequests
                .Include(t => t.TargetBranch)
                .Include(t => t.Items)
                .AsQueryable();

            if (role != "Admin")
                query = query.Where(t => t.RequesterBranchId == userBranchId);

            var transfers = await query
                .Select(t => new
                {
                    t.Id,
                    t.Status,
                    HedefSube = t.TargetBranch.Name,
                    KargoSayisi = t.Items.Count,
                    t.RequestedAt,
                    t.ScheduledAt
                })
                .OrderByDescending(t => t.RequestedAt)
                .ToListAsync();

            return Ok(transfers);
        }

        // Transfer talebini onayla
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(
            int id, [FromBody] ApproveTransferRequest request)
        {
            var branchIdClaim = User.FindFirst("branchId")?.Value;
            int.TryParse(branchIdClaim, out int userBranchId);
            var role = User.FindFirst(
                System.Security.Claims.ClaimTypes.Role)?.Value;

            var transfer = await _context.TransferRequests
                .Include(t => t.Items)
                .Include(t => t.RequesterBranch)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transfer == null)
                return NotFound(new { message = "Transfer talebi bulunamadı." });

            if (transfer.Status != "Bekliyor")
                return BadRequest(new
                {
                    message = "Sadece bekleyen talepler onaylanabilir."
                });

            // Sadece hedef şube onaylayabilir
            if (role != "Admin" && transfer.TargetBranchId != userBranchId)
                return Forbid();

            var userIdClaim = User.FindFirst("userId")?.Value;
            int.TryParse(userIdClaim, out int userId);

            transfer.Status = "Onaylandı";
            transfer.RespondedByUserId = userId;
            transfer.RespondedAt = DateTime.UtcNow;
            transfer.ScheduledAt = request.ScheduledAt;

            // Kargoları hedef şubeye taşı
            var shipmentIds = transfer.Items.Select(i => i.ShipmentId).ToList();
            var shipments = await _context.Shipments
                .Where(s => shipmentIds.Contains(s.Id))
                .ToListAsync();

            foreach (var shipment in shipments)
            {
                shipment.BranchId = transfer.TargetBranchId;
                shipment.UpdatedAt = DateTime.UtcNow;
            }

            // Talep eden şubeye bildirim gönder
            var notification = new Notification
            {
                TransferRequestId = transfer.Id,
                BranchId = transfer.RequesterBranchId,
                Message = $"Transfer talebiniz onaylandı. " +
                    $"Planlanan tarih: {request.ScheduledAt:dd.MM.yyyy HH:mm}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _producer.PublishAsync("transfer_talebi_onaylandi", new
            {
                TransferRequestId = transfer.Id,
                ScheduledAt = request.ScheduledAt,
                KargoSayisi = shipmentIds.Count
            });

            _logger.LogInformation(
                "Transfer talebi onaylandı: {Id}", transfer.Id);

            return Ok(new
            {
                transfer.Id,
                transfer.Status,
                transfer.ScheduledAt,
                message = "Transfer talebi onaylandı, kargolar devredildi."
            });
        }

        // Transfer talebini reddet
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(
            int id, [FromBody] RejectTransferRequest request)
        {
            var branchIdClaim = User.FindFirst("branchId")?.Value;
            int.TryParse(branchIdClaim, out int userBranchId);
            var role = User.FindFirst(
                System.Security.Claims.ClaimTypes.Role)?.Value;

            var transfer = await _context.TransferRequests
                .Include(t => t.RequesterBranch)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transfer == null)
                return NotFound(new { message = "Transfer talebi bulunamadı." });

            if (transfer.Status != "Bekliyor")
                return BadRequest(new
                {
                    message = "Sadece bekleyen talepler reddedilebilir."
                });

            if (role != "Admin" && transfer.TargetBranchId != userBranchId)
                return Forbid();

            var userIdClaim = User.FindFirst("userId")?.Value;
            int.TryParse(userIdClaim, out int userId);

            transfer.Status = "Reddedildi";
            transfer.RespondedByUserId = userId;
            transfer.RespondedAt = DateTime.UtcNow;
            transfer.Note = request.Reason;

            // Talep eden şubeye bildirim gönder
            var notification = new Notification
            {
                TransferRequestId = transfer.Id,
                BranchId = transfer.RequesterBranchId,
                Message = $"Transfer talebiniz reddedildi. " +
                    $"Sebep: {request.Reason}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _producer.PublishAsync("transfer_talebi_reddedildi", new
            {
                TransferRequestId = transfer.Id,
                Reason = request.Reason
            });

            _logger.LogInformation(
                "Transfer talebi reddedildi: {Id}", transfer.Id);

            return Ok(new
            {
                transfer.Id,
                transfer.Status,
                message = "Transfer talebi reddedildi."
            });
        }
    }

    public class CreateTransferRequest
    {
        public int RequesterBranchId { get; set; }
        public int TargetBranchId { get; set; }
        public List<int> ShipmentIds { get; set; } = new();
        public string? Note { get; set; }
    }

    public class ApproveTransferRequest
    {
        public DateTime ScheduledAt { get; set; }
    }

    public class RejectTransferRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}