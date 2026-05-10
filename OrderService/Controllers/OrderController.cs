using FluentValidation;
using KargoTakip.Infrastructure.Data;
using KargoTakip.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Messaging;
using System.Net.Http.Json;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly KargoTakipDbContext _context;
        private readonly RabbitMqProducer _producer;
        private readonly ILogger<OrderController> _logger;
        private readonly IValidator<CreateShipmentRequest> _validator;
        private readonly IConfiguration _configuration;

        public OrderController(
            KargoTakipDbContext context,
            RabbitMqProducer producer,
            ILogger<OrderController> logger,
            IValidator<CreateShipmentRequest> validator,
            IConfiguration configuration)
        {
            _context = context;
            _producer = producer;
            _logger = logger;
            _validator = validator;
            _configuration = configuration;
        }

        // Tüm kargoları listele
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var shipments = await _context.Shipments
                .Include(s => s.ReceiverCity)
                .Include(s => s.Branch)
                .Include(s => s.CreatedByUser)
                .Include(s => s.AssignedVehicle)
                .Select(s => new
                {
                    s.Id,
                    s.TrackingCode,
                    s.SenderName,
                    s.ReceiverName,
                    s.ReceiverAddress,
                    ReceiverCity = s.ReceiverCity.Name,
                    s.Weight,
                    s.Priority,
                    s.CurrentStatus,
                    Branch = s.Branch.Name,
                    AssignedVehicle = s.AssignedVehicle != null ? s.AssignedVehicle.PlateNumber : null,
                    CreatedBy = s.CreatedByUser.FullName,
                    s.CreatedAt
                })
                .ToListAsync();

            return Ok(shipments);
        }

        // Tek kargo getir
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var shipment = await _context.Shipments
                .Include(s => s.ReceiverCity)
                .Include(s => s.Branch)
                .Include(s => s.CreatedByUser)
                .Include(s => s.AssignedVehicle)
                .Include(s => s.StatusHistories)
                .Where(s => s.Id == id)
                .Select(s => new
                {
                    s.Id,
                    s.TrackingCode,
                    s.SenderName,
                    s.ReceiverName,
                    s.ReceiverAddress,
                    ReceiverCity = s.ReceiverCity.Name,
                    s.Weight,
                    s.Priority,
                    s.CurrentStatus,
                    Branch = s.Branch.Name,
                    AssignedVehicle = s.AssignedVehicle != null ? s.AssignedVehicle.PlateNumber : null,
                    CreatedBy = s.CreatedByUser.FullName,
                    s.CreatedAt,
                    s.UpdatedAt,
                    StatusHistory = s.StatusHistories.Select(h => new
                    {
                        h.Status,
                        h.Note,
                        h.ServiceSource,
                        h.ChangedAt
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (shipment == null)
                return NotFound(new { message = "Kargo bulunamadı." });

            return Ok(shipment);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var shipment = await _context.Shipments.FindAsync(id);
            if (shipment == null)
                return NotFound(new { message = "Kargo bulunamadı." });

            var eskiDurum = shipment.CurrentStatus;
            shipment.CurrentStatus = request.NewStatus;
            shipment.UpdatedAt = DateTime.UtcNow;

            var statusHistory = new ShipmentStatusHistory
            {
                ShipmentId = shipment.Id,
                Status = request.NewStatus,
                Note = request.Note,
                ServiceSource = request.ServiceSource ?? "OrderService",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = request.ChangedByUserId
            };

            _context.ShipmentStatusHistories.Add(statusHistory);
            await _context.SaveChangesAsync();

            // RabbitMQ'ya event yayınla
            var durumuGuncellendiEvent = new KargoDurumuGuncellendiEvent
            {
                ShipmentId = shipment.Id,
                TrackingCode = shipment.TrackingCode,
                EskiDurum = eskiDurum,
                YeniDurum = request.NewStatus,
                BranchId = shipment.BranchId,
                GuncellemeTarihi = DateTime.UtcNow
            };

            await _producer.PublishAsync("kargo_durumu_guncellendi", durumuGuncellendiEvent);

            return Ok(new
            {
                shipment.Id,
                shipment.TrackingCode,
                eskiDurum,
                yeniDurum = request.NewStatus,
                message = "Durum güncellendi."
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateShipmentRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors
                    .Select(e => new { field = e.PropertyName, message = e.ErrorMessage }));

            _logger.LogInformation("Yeni kargo oluşturuluyor: {SenderName} -> {ReceiverName}",
                request.SenderName, request.ReceiverName);

            // Şubenin var olup olmadığını kontrol et
            var branch = await _context.Branches.FindAsync(request.BranchId);
            if (branch == null)
                return BadRequest(new { message = "Şube bulunamadı." });

            // Şehrin var olup olmadığını kontrol et
            var city = await _context.Cities.FindAsync(request.ReceiverCityId);
            if (city == null)
                return BadRequest(new { message = "Şehir bulunamadı." });

            // Kullanıcının var olup olmadığını kontrol et
            var user = await _context.Users.FindAsync(request.CreatedByUserId);
            if (user == null)
                return BadRequest(new { message = "Kullanıcı bulunamadı." });

            // Takip kodu üret
            var trackingCode = "KRG-" + DateTime.UtcNow.Ticks.ToString().Substring(10, 8);

            var shipment = new Shipment
            {
                TrackingCode = trackingCode,
                SenderName = request.SenderName,
                ReceiverName = request.ReceiverName,
                ReceiverAddress = request.ReceiverAddress,
                ReceiverCityId = request.ReceiverCityId,
                Weight = request.Weight,
                Priority = request.Priority ?? "Normal",
                CurrentStatus = "Hazırlanıyor",
                BranchId = request.BranchId,
                CreatedByUserId = request.CreatedByUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Shipments.Add(shipment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Kargo oluşturuldu: {TrackingCode}", shipment.TrackingCode);

            // İlk durum geçmişini kaydet
            var statusHistory = new ShipmentStatusHistory
            {
                ShipmentId = shipment.Id,
                Status = "Hazırlanıyor",
                Note = "Kargo sisteme oluşturuldu.",
                ServiceSource = "OrderService",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = request.CreatedByUserId
            };

            _context.ShipmentStatusHistories.Add(statusHistory);
            await _context.SaveChangesAsync();

            // Araç atama isteği gönder
            var assignRequest = new
            {
                cityId = request.ReceiverCityId,
                requiredCapacity = 1
            };

            try
            {
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                var httpClient2 = new HttpClient(handler);

                var vehicleServiceUrl = _configuration["VehicleService:BaseUrl"]
    ?? "https://localhost:7139";

                var response = await httpClient2.PostAsJsonAsync(
                    $"{vehicleServiceUrl}/api/vehicles/assign",
                    assignRequest
                );

                if (response.IsSuccessStatusCode)
                {
                    var assignResult = await response.Content
                        .ReadFromJsonAsync<AssignResult>();

                    if (assignResult != null)
                    {
                        shipment.AssignedVehicleId = assignResult.VehicleId;
                        shipment.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();

                        _logger.LogInformation("Araç atandı: {TrackingCode} -> {VehicleId}",
                            shipment.TrackingCode, assignResult.VehicleId);
                    }
                }
            }
            catch (Exception ex)
            {
                // Araç ataması başarısız olsa bile kargo oluşturulur
                // İleride loglama buraya eklenecek
            }

            var kargoEvent = new KargoOlusturulduEvent
            {
                ShipmentId = shipment.Id,
                TrackingCode = shipment.TrackingCode,
                ReceiverName = shipment.ReceiverName,
                CurrentStatus = shipment.CurrentStatus,
                BranchId = shipment.BranchId,
                OlusturulmaTarihi = shipment.CreatedAt
            };

            await _producer.PublishAsync("kargo_olusturuldu", kargoEvent);

            return CreatedAtAction(nameof(GetById), new { id = shipment.Id }, new
            {
                shipment.Id,
                shipment.TrackingCode,
                shipment.CurrentStatus,
                message = "Kargo başarıyla oluşturuldu."
            });
        }
    }

    public class CreateShipmentRequest
    {
        public string SenderName { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverAddress { get; set; } = string.Empty;
        public int ReceiverCityId { get; set; }
        public decimal Weight { get; set; }
        public string? Priority { get; set; }
        public int BranchId { get; set; }
        public int CreatedByUserId { get; set; }
    }

    public class UpdateStatusRequest
    {
        public string NewStatus { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? ServiceSource { get; set; }
        public int ChangedByUserId { get; set; }
    }
    public class AssignResult
    {
        public int VehicleId { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
    }
}