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
        private readonly IValidator<UpdateStatusRequest> _statusValidator;
        private readonly IHttpClientFactory _httpClientFactory;

        public OrderController(
            KargoTakipDbContext context,
            RabbitMqProducer producer,
            ILogger<OrderController> logger,
            IValidator<CreateShipmentRequest> validator,
            IValidator<UpdateStatusRequest> statusValidator,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _producer = producer;
            _logger = logger;
            _validator = validator;
            _statusValidator = statusValidator;
            _httpClientFactory = httpClientFactory;
        }

        // Token'daki kimlik bilgileri — istemciden gelen degerlere guvenilmez.
        private string? CurrentRole =>
            User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        private int CurrentUserId
        {
            get
            {
                int.TryParse(User.FindFirst("userId")?.Value, out int userId);
                return userId;
            }
        }

        private int CurrentBranchId
        {
            get
            {
                int.TryParse(User.FindFirst("branchId")?.Value, out int branchId);
                return branchId;
            }
        }

        // Sayfalama parametrelerini normalize eder (varsayilan 50, tavan 200).
        private static (int Sayfa, int Boyut) SayfaParametreleri(int? sayfa, int? boyut)
        {
            var s = sayfa.GetValueOrDefault(1);
            var b = boyut.GetValueOrDefault(50);
            return (s < 1 ? 1 : s, b < 1 ? 50 : (b > 200 ? 200 : b));
        }

        // Tüm kargoları listele
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? sayfa, [FromQuery] int? sayfaBoyutu)
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var branchIdClaim = User.FindFirst("branchId")?.Value;
            int.TryParse(branchIdClaim, out int userBranchId);

            var query = _context.Shipments
                .Include(s => s.ReceiverCity)
                .Include(s => s.Branch)
                .Include(s => s.CreatedByUser)
                .Include(s => s.AssignedVehicle)
                .AsQueryable();

            // Admin her şeyi görür, diğerleri sadece kendi şubesini
            if (role != "Admin")
                query = query.Where(s => s.BranchId == userBranchId);

            // Onceden tum tablo cekiliyordu.
            var (aktifSayfa, boyut) = SayfaParametreleri(sayfa, sayfaBoyutu);
            var toplamKayit = await query.CountAsync();

            var shipments = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((aktifSayfa - 1) * boyut)
                .Take(boyut)
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
                    AssignedVehicle = s.AssignedVehicle != null
                        ? s.AssignedVehicle.PlateNumber : null,
                    CreatedBy = s.CreatedByUser.FullName,
                    s.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                toplamKayit,
                sayfa = aktifSayfa,
                sayfaBoyutu = boyut,
                kayitlar = shipments
            });
        }

        [HttpGet("cities")]
        public async Task<IActionResult> GetCities()
        {
            var cities = await _context.Cities
                .Select(c => new { c.Id, c.Name })
                .OrderBy(c => c.Name)
                .ToListAsync();
            return Ok(cities);
        }

        // Tek kargo getir
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var branchIdClaim = User.FindFirst("branchId")?.Value;
            int.TryParse(branchIdClaim, out int userBranchId);

            var shipment = await _context.Shipments
                .Include(s => s.ReceiverCity)
                .Include(s => s.Branch)
                .Include(s => s.CreatedByUser)
                .Include(s => s.AssignedVehicle)
                .Include(s => s.StatusHistories)
                .Where(s => s.Id == id)
                .Where(s => role == "Admin" || s.BranchId == userBranchId)
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
                    AssignedVehicle = s.AssignedVehicle != null
                        ? s.AssignedVehicle.PlateNumber : null,
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

        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var validationResult = await _statusValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors
                    .Select(e => new { field = e.PropertyName, message = e.ErrorMessage }));

            var shipment = await _context.Shipments.FindAsync(id);
            if (shipment == null)
                return NotFound(new { message = "Kargo bulunamadı." });

            // Baska subenin kargosu degistirilemez (GetAll/GetById ile ayni kural)
            if (CurrentRole != "Admin" && shipment.BranchId != CurrentBranchId)
                return Forbid();

            var eskiDurum = shipment.CurrentStatus;
            shipment.CurrentStatus = request.NewStatus;
            shipment.UpdatedAt = DateTime.UtcNow;

            // Dağıtıma çıkınca teslimat kodu üret
            if (request.NewStatus == "Dağıtımda" && string.IsNullOrEmpty(shipment.DeliveryCode))
            {
                shipment.DeliveryCode = GenerateDeliveryCode();
                shipment.DeliveryCodeExpiry = DateTime.UtcNow.AddHours(24);
                shipment.DeliveryCodeUsed = false;
            }

            // Teslim edilince kodu kullanıldı işaretle
            if (request.NewStatus == "Teslim Edildi")
            {
                shipment.DeliveryCodeUsed = true;
            }

            // Terminal duruma ilk gecarken aracin yuku dusurulur.
            var terminalDurumlar = new[] { "Teslim Edildi", "İptal" };
            if (terminalDurumlar.Contains(request.NewStatus) &&
                !terminalDurumlar.Contains(eskiDurum))
            {
                await AracYukunuSerbestBirakAsync(shipment);
            }

            var statusHistory = new ShipmentStatusHistory
            {
                ShipmentId = shipment.Id,
                Status = request.NewStatus,
                Note = request.Note,
                ServiceSource = request.ServiceSource ?? "OrderService",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = CurrentUserId
            };

            _context.ShipmentStatusHistories.Add(statusHistory);
            await _context.SaveChangesAsync();

            var durumuGuncellendiEvent = new KargoDurumuGuncellendiEvent
            {
                ShipmentId = shipment.Id,
                TrackingCode = shipment.TrackingCode,
                EskiDurum = eskiDurum,
                YeniDurum = request.NewStatus,
                BranchId = shipment.BranchId,
                GuncellemeTarihi = DateTime.UtcNow,
                ReceiverEmail = shipment.ReceiverEmail,
                ReceiverName = shipment.ReceiverName,
                DeliveryCode = shipment.DeliveryCode
            };

            await _producer.PublishAsync("kargo_durumu_guncellendi", durumuGuncellendiEvent);

            return Ok(new
            {
                shipment.Id,
                shipment.TrackingCode,
                eskiDurum,
                yeniDurum = request.NewStatus,
                deliveryCode = shipment.DeliveryCode,
                message = "Durum güncellendi."
            });
        }

        private string GenerateDeliveryCode() =>
            OrderService.Services.KodUretici.TeslimatKodu();

        // Ticks tabanli eski uretim cakisabiliyordu. Kriptografik rastgele
        // uretip DB'de dogrular; unique index ikinci savunma hattidir.
        private async Task<string> GenerateUniqueTrackingCodeAsync()
        {
            for (int deneme = 0; deneme < 5; deneme++)
            {
                var kod = OrderService.Services.KodUretici.TakipKodu();

                if (!await _context.Shipments.AnyAsync(x => x.TrackingCode == kod))
                    return kod;
            }

            throw new InvalidOperationException(
                "Benzersiz takip kodu uretilemedi.");
        }

        // Kargo terminal duruma gecerken tasidigi aracin yukunu geri birakir.
        // Bu yapilmazsa CurrentLoad hic azalmaz ve tum filo zamanla dolu gorunur.
        private async Task AracYukunuSerbestBirakAsync(Shipment shipment)
        {
            if (shipment.AssignedVehicleId == null)
                return;

            var vehicle = await _context.Vehicles.FindAsync(shipment.AssignedVehicleId.Value);
            if (vehicle == null)
                return;

            vehicle.CurrentLoad = Math.Max(0, vehicle.CurrentLoad - 1);
            if (vehicle.CurrentLoad < vehicle.Capacity)
                vehicle.IsAvailable = true;

            _logger.LogInformation(
                "Arac {VehicleId} yuku serbest birakildi: {Load}/{Capacity}",
                vehicle.Id, vehicle.CurrentLoad, vehicle.Capacity);

            await KonsolidasyonPlaniniKapatAsync(shipment.Id);
        }

        // Plan Status'u hicbir zaman "Planlandi"dan cikmiyordu. Plandaki tum
        // kargolar terminal duruma geldiyse plan tamamlanmis sayilir.
        private async Task KonsolidasyonPlaniniKapatAsync(int shipmentId)
        {
            var planIds = await _context.ConsolidationPlanItems
                .Where(i => i.ShipmentId == shipmentId)
                .Select(i => i.ConsolidationPlanId)
                .ToListAsync();

            foreach (var planId in planIds)
            {
                var plan = await _context.ConsolidationPlans.FindAsync(planId);
                if (plan == null || plan.Status == "Tamamlandı")
                    continue;

                // Bu metot SaveChanges'ten ONCE calisir; sorgu DB'yi okudugu icin
                // isleme konu kargonun yeni durumu henuz gorunmez. Bu yuzden o
                // kargo sorgudan haric tutulur, terminal oldugu zaten bilinmektedir.
                var kalanVar = await _context.ConsolidationPlanItems
                    .Where(i => i.ConsolidationPlanId == planId &&
                                i.ShipmentId != shipmentId)
                    .Join(_context.Shipments,
                        i => i.ShipmentId, sh => sh.Id, (i, sh) => sh.CurrentStatus)
                    .AnyAsync(durum => durum != "Teslim Edildi" && durum != "İptal");

                if (!kalanVar)
                {
                    plan.Status = "Tamamlandı";
                    _logger.LogInformation(
                        "Konsolidasyon plani {PlanId} tamamlandi.", planId);
                }
            }
        }

        [HttpPut("{id:int}/deliver")]
        public async Task<IActionResult> Deliver(int id, [FromBody] DeliverRequest request)
        {
            var shipment = await _context.Shipments.FindAsync(id);
            if (shipment == null)
                return NotFound(new { message = "Kargo bulunamadı." });

            if (CurrentRole != "Admin" && shipment.BranchId != CurrentBranchId)
                return Forbid();

            if (shipment.CurrentStatus == "Teslim Edildi")
                return BadRequest(new { message = "Bu kargo zaten teslim edildi." });

            if (string.IsNullOrEmpty(shipment.DeliveryCode))
                return BadRequest(new { message = "Bu kargo için teslimat kodu oluşturulmamış." });

            if (shipment.DeliveryCodeUsed)
                return BadRequest(new { message = "Teslimat kodu daha önce kullanıldı." });

            if (shipment.DeliveryCodeExpiry < DateTime.UtcNow)
                return BadRequest(new { message = "Teslimat kodunun süresi dolmuş." });

            if (shipment.DeliveryCode != request.DeliveryCode)
                return BadRequest(new { message = "Teslimat kodu hatalı." });

            shipment.CurrentStatus = "Teslim Edildi";
            shipment.DeliveryCodeUsed = true;
            shipment.UpdatedAt = DateTime.UtcNow;

            await AracYukunuSerbestBirakAsync(shipment);

            var statusHistory = new ShipmentStatusHistory
            {
                ShipmentId = shipment.Id,
                Status = "Teslim Edildi",
                Note = "Teslimat kodu doğrulandı.",
                ServiceSource = "CourierApp",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = CurrentUserId
            };

            _context.ShipmentStatusHistories.Add(statusHistory);
            await _context.SaveChangesAsync();

            var durumuGuncellendiEvent = new KargoDurumuGuncellendiEvent
            {
                ShipmentId = shipment.Id,
                TrackingCode = shipment.TrackingCode,
                EskiDurum = "Dağıtımda",
                YeniDurum = "Teslim Edildi",
                BranchId = shipment.BranchId,
                GuncellemeTarihi = DateTime.UtcNow,
                ReceiverEmail = shipment.ReceiverEmail,
                ReceiverName = shipment.ReceiverName,
                DeliveryCode = shipment.DeliveryCode
            };

            await _producer.PublishAsync("kargo_durumu_guncellendi", durumuGuncellendiEvent);

            _logger.LogInformation("Kargo teslim edildi: {TrackingCode}", shipment.TrackingCode);

            return Ok(new { message = "Kargo başarıyla teslim edildi.", shipment.TrackingCode });
        }

        // Müşteri takip - token gerektirmez
        [AllowAnonymous]
        [HttpGet("track/{trackingCode}")]
        public async Task<IActionResult> Track(string trackingCode)
        {
            var shipment = await _context.Shipments
                .Include(s => s.ReceiverCity)
                .Include(s => s.Branch)
                .Include(s => s.StatusHistories)
                .Where(s => s.TrackingCode == trackingCode)
                .Select(s => new
                {
                    s.TrackingCode,
                    s.ReceiverName,
                    s.ReceiverAddress,
                    ReceiverCity = s.ReceiverCity.Name,
                    s.Weight,
                    s.Priority,
                    s.CurrentStatus,
                    Branch = s.Branch.Name,
                    s.CreatedAt,
                    s.UpdatedAt,
                    StatusHistory = s.StatusHistories
                        .OrderBy(h => h.ChangedAt)
                        .Select(h => new
                        {
                            h.Status,
                            h.Note,
                            h.ChangedAt
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (shipment == null)
                return NotFound(new { message = "Kargo bulunamadı." });

            return Ok(shipment);
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

            // Şube ve kullanıcı token'dan alınır; yalnızca Admin başka şube adına kargo açabilir.
            var branchId = CurrentBranchId;
            if (CurrentRole == "Admin" && request.BranchId.HasValue)
                branchId = request.BranchId.Value;

            var createdByUserId = CurrentUserId;
            if (branchId <= 0 || createdByUserId <= 0)
                return Unauthorized(new { message = "Token'da şube veya kullanıcı bilgisi yok." });

            // Şubenin var olup olmadığını kontrol et
            var branch = await _context.Branches.FindAsync(branchId);
            if (branch == null)
                return BadRequest(new { message = "Şube bulunamadı." });

            // Şehrin var olup olmadığını kontrol et
            var city = await _context.Cities.FindAsync(request.ReceiverCityId);
            if (city == null)
                return BadRequest(new { message = "Şehir bulunamadı." });

            // Kullanıcının var olup olmadığını kontrol et
            var user = await _context.Users.FindAsync(createdByUserId);
            if (user == null)
                return BadRequest(new { message = "Kullanıcı bulunamadı." });

            // Takip kodu üret
            var trackingCode = await GenerateUniqueTrackingCodeAsync();

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
                BranchId = branchId,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ReceiverEmail = request.ReceiverEmail
            };

            // Kargo ve ilk durum gecmisi tek SaveChanges ile yazilir; iki ayri
            // cagri arasinda hata olursa gecmissiz kargo kaliyordu.
            var statusHistory = new ShipmentStatusHistory
            {
                Shipment = shipment,
                Status = "Hazırlanıyor",
                Note = "Kargo sisteme oluşturuldu.",
                ServiceSource = "OrderService",
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = createdByUserId
            };

            _context.Shipments.Add(shipment);
            _context.ShipmentStatusHistories.Add(statusHistory);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Kargo oluşturuldu: {TrackingCode}", shipment.TrackingCode);

            // Araç atama isteği gönder
            var assignRequest = new
            {
                cityId = request.ReceiverCityId,
                requiredCapacity = 1
            };

            try
            {
                var httpClient = _httpClientFactory.CreateClient("vehicle-service");

                // Cagiranin token'ini ilet: VehicleService/assign artik kimlik dogrulamasi istiyor.
                var authHeader = Request.Headers.Authorization.ToString();
                if (!string.IsNullOrEmpty(authHeader))
                    httpClient.DefaultRequestHeaders.Add("Authorization", authHeader);

                var response = await httpClient.PostAsJsonAsync(
                    "api/vehicles/assign",
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
                // Araç ataması başarısız olsa bile kargo oluşturulur.
                _logger.LogWarning(ex,
                    "Araç atama isteği başarısız: {TrackingCode}", shipment.TrackingCode);
            }

            var kargoEvent = new KargoOlusturulduEvent
            {
                ShipmentId = shipment.Id,
                TrackingCode = shipment.TrackingCode,
                ReceiverName = shipment.ReceiverName,
                ReceiverEmail = shipment.ReceiverEmail,
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
        // Sadece Admin baska bir sube adina kargo acabilir; bos birakilirsa token'daki sube kullanilir.
        public int? BranchId { get; set; }
        public string? ReceiverEmail { get; set; }
    }

    public class UpdateStatusRequest
    {
        public string NewStatus { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? ServiceSource { get; set; }
    }
    public class AssignResult
    {
        public int VehicleId { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
    }
    public class DeliverRequest
    {
        public string DeliveryCode { get; set; } = string.Empty;
    }
}