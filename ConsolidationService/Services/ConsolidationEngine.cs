using KargoTakip.Infrastructure.Data;
using KargoTakip.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsolidationService.Services
{
    public class ConsolidationEngine
    {
        private readonly KargoTakipDbContext _context;
        private readonly ILogger<ConsolidationEngine> _logger;
        private readonly IConfiguration _configuration;

        private int OccupancyThreshold =>
            _configuration.GetValue<int>("Consolidation:OccupancyThreshold", 70);
        private int MaxWaitHours =>
            _configuration.GetValue<int>("Consolidation:MaxWaitHours", 48);

        public ConsolidationEngine(
            KargoTakipDbContext context,
            ILogger<ConsolidationEngine> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task RunAsync()
        {
            _logger.LogInformation("Konsolidasyon motoru çalışıyor: {Time}",
                DateTime.UtcNow);

            // Tüm aktif şubeleri al
            var branches = await _context.Branches
                .Where(b => b.IsActive)
                .ToListAsync();

            foreach (var branch in branches)
            {
                await ProcessBranchAsync(branch);
            }

            await PlanDurumlariniIlerletAsync();
        }

        /// <summary>
        /// Planlanan kalkis saati gelen planlari yola cikmis olarak isaretler.
        /// Onceden plan "Planlandi"dan dogrudan "Tamamlandi"ya geciyor,
        /// ActualDepartureAt ise hic yazilmiyordu.
        /// </summary>
        private async Task PlanDurumlariniIlerletAsync()
        {
            var simdi = DateTime.UtcNow;

            var kalkacakPlanlar = await _context.ConsolidationPlans
                .Where(p => p.Status == ConsolidationPlanStatus.Planlandi &&
                            p.PlannedDepartureAt <= simdi)
                .ToListAsync();

            if (kalkacakPlanlar.Count == 0)
                return;

            foreach (var plan in kalkacakPlanlar)
            {
                plan.Status = ConsolidationPlanStatus.Yolda;
                plan.ActualDepartureAt = simdi;

                _logger.LogInformation(
                    "Konsolidasyon plani {PlanId} yola cikti.", plan.Id);
            }

            await _context.SaveChangesAsync();
        }

        private async Task ProcessBranchAsync(Branch branch)
        {
            // Bu şubede bekleyen kargoları al
            var pendingShipments = await _context.Shipments
                .Where(s => s.BranchId == branch.Id &&
                            s.CurrentStatus == ShipmentStatus.Hazirlaniyor &&
                            s.AssignedVehicle == null)
                .Include(s => s.ReceiverCity)
                .ToListAsync();

            if (!pendingShipments.Any()) return;

            // Kargoları hedef şehre göre grupla
            var groups = pendingShipments
                .GroupBy(s => s.ReceiverCityId)
                .ToList();

            foreach (var group in groups)
            {
                await ProcessShipmentGroupAsync(branch, group.Key, group.ToList());
            }
        }

        private async Task ProcessShipmentGroupAsync(
            Branch branch, int destinationCityId, List<Shipment> shipments)
        {
            // Bu şubede Intercity veya Both tipinde müsait araç var mı
            var vehicle = await _context.Vehicles
                .Include(v => v.VehicleType)
                .Where(v => v.BranchId == branch.Id &&
                            v.IsAvailable &&
                            (v.VehicleType.RouteType == "Intercity" ||
                             v.VehicleType.RouteType == "Both"))
                .OrderByDescending(v => v.Capacity)
                .FirstOrDefaultAsync();

            if (vehicle == null)
            {
                _logger.LogWarning(
                    "Şube {BranchId} için uygun araç bulunamadı.", branch.Id);
                return;
            }

            var totalCapacity = vehicle.Capacity;

            // Kapasite paket adedi cinsindendir (OrderService de requiredCapacity=1 gonderir).
            // Gruptaki kargo sayisi kapasiteyi asarsa fazlasi bu sefere alinmaz;
            // aksi halde doluluk %100'u asip OccupancyRate precision(5,2) tasardi.
            if (shipments.Count > totalCapacity)
            {
                _logger.LogInformation(
                    "Şube {BranchId} → Şehir {CityId}: {Total} kargodan {Fit} tanesi " +
                    "bu sefere alınıyor, kalanı sonraki seferde değerlendirilecek.",
                    branch.Id, destinationCityId, shipments.Count, totalCapacity);

                shipments = shipments
                    .OrderBy(s => s.CreatedAt)
                    .Take(totalCapacity)
                    .ToList();
            }

            var usedCapacity = shipments.Count;
            var occupancyRate = (double)usedCapacity / totalCapacity * 100;

            var planItems = shipments
                .Select(s => new { Shipment = s, Reason = "PrimaryDestination" })
                .ToList();

            // Doluluk eşiği kontrolü
            if (occupancyRate < OccupancyThreshold)
            {
                // En eski kargo ne zaman eklendi?
                var oldestShipment = shipments
                    .OrderBy(s => s.CreatedAt)
                    .First();
                var waitHours = (DateTime.UtcNow - oldestShipment.CreatedAt).TotalHours;

                if (waitHours < MaxWaitHours)
                {
                    _logger.LogInformation(
                        "Şube {BranchId} → Şehir {CityId}: " +
                        "Doluluk {Rate:F1}%, {Hours:F1} saat bekliyor. Henüz bekle.",
                        branch.Id, destinationCityId, occupancyRate, waitHours);
                    return;
                }

                // 48 saat geçti, komşu şehir algoritması devreye gir
                _logger.LogInformation(
                    "Şube {BranchId}: 48 saat geçti, komşu şehir algoritması devreye giriyor.",
                    branch.Id);

                var neighborShipments = await GetNeighborShipmentsAsync(
                    branch, destinationCityId, totalCapacity - usedCapacity, shipments);

                foreach (var ns in neighborShipments)
                {
                    planItems.Add(new { Shipment = ns, Reason = "NeighborCity" });
                }

                usedCapacity = planItems.Count;
                occupancyRate = (double)usedCapacity / totalCapacity * 100;
            }

            // Sefer planı oluştur
            await CreateConsolidationPlanAsync(
                vehicle, branch, destinationCityId,
                planItems.Select(p => (p.Shipment, p.Reason)).ToList(),
                totalCapacity, usedCapacity, occupancyRate);
        }

        private async Task<List<Shipment>> GetNeighborShipmentsAsync(
            Branch branch, int destinationCityId,
            int remainingCapacity, List<Shipment> alreadyIncluded)
        {
            var result = new List<Shipment>();
            if (remainingCapacity <= 0) return result;

            var alreadyIncludedIds = alreadyIncluded.Select(s => s.Id).ToHashSet();

            // Hedef şehrin komşularını mesafeye göre sırala
            var neighbors = await _context.CityDistances
                .Where(cd => cd.FromCityId == destinationCityId)
                .OrderBy(cd => cd.DistanceKm)
                .Take(3) // En yakın 3 komşu
                .ToListAsync();

            foreach (var neighbor in neighbors)
            {
                if (result.Count >= remainingCapacity) break;

                // Bu komşuya gidecek bekleyen kargolar
                var neighborShipments = await _context.Shipments
                    .Where(s => s.BranchId == branch.Id &&
                                s.ReceiverCityId == neighbor.ToCityId &&
                                s.CurrentStatus == ShipmentStatus.Hazirlaniyor &&
                                s.AssignedVehicle == null &&
                                !alreadyIncludedIds.Contains(s.Id))
                    .Take(remainingCapacity - result.Count)
                    .ToListAsync();

                result.AddRange(neighborShipments);

                _logger.LogInformation(
                    "Komşu şehir eklendi: {CityId}, {Count} kargo, mesafe: {Km}km",
                    neighbor.ToCityId, neighborShipments.Count, neighbor.DistanceKm);
            }

            return result;
        }

        private async Task CreateConsolidationPlanAsync(
            Vehicle vehicle,
            Branch branch,
            int destinationCityId,
            List<(Shipment Shipment, string Reason)> items,
            int totalCapacity,
            int usedCapacity,
            double occupancyRate)
        {
            // Yakit tasarrufu hesabi.
            // Onceki formul (1 - doluluk) ile carpiyordu; yani arac ne kadar BOS
            // olursa tasarruf o kadar yuksek cikiyordu. Konsolidasyonun mantigi
            // bunun tersi: tasarruf, yapilmayan ayri seferlerden gelir.
            // Baz senaryo: her kargo ayri seferle gitseydi -> items.Count sefer.
            // Konsolide senaryo: tek sefer. Tasarruf = (sefer sayisi - 1) x sefer maliyeti.
            var distance = await _context.CityDistances
                .Where(cd => cd.FromCityId == branch.CityId &&
                             cd.ToCityId == destinationCityId)
                .FirstOrDefaultAsync();

            var fuelSaving = distance == null
                ? 0m
                : KonsolidasyonHesap.YakitTasarrufu(distance.DistanceKm, items.Count);

            var plan = new ConsolidationPlan
            {
                VehicleId = vehicle.Id,
                OriginBranchId = branch.Id,
                DestinationCityId = destinationCityId,
                Status = ConsolidationPlanStatus.Planlandi,
                PlannedDepartureAt = DateTime.UtcNow.AddHours(2),
                TotalCapacity = totalCapacity,
                UsedCapacity = usedCapacity,
                OccupancyRate = KonsolidasyonHesap.DolulukOrani(usedCapacity, totalCapacity),
                EstimatedFuelSaving = fuelSaving,
                CreatedAt = DateTime.UtcNow
            };

            _context.ConsolidationPlans.Add(plan);
            await _context.SaveChangesAsync();

            // Plan kalemlerini ekle
            var planItems = items.Select(i => new ConsolidationPlanItem
            {
                ConsolidationPlanId = plan.Id,
                ShipmentId = i.Shipment.Id,
                AddedReason = i.Reason
            }).ToList();

            _context.ConsolidationPlanItems.AddRange(planItems);

            // Araci mesgul et. CurrentLoad'un uzerine YAZILMAZ; aracin mevcut
            // yukune eklenir, aksi halde VehicleService'in atadigi yuk kayboluyordu.
            vehicle.CurrentLoad = Math.Min(
                vehicle.Capacity, vehicle.CurrentLoad + usedCapacity);
            vehicle.IsAvailable = vehicle.CurrentLoad < vehicle.Capacity;

            // Kargoları "Yolda" yap
            foreach (var (shipment, _) in items)
            {
                shipment.CurrentStatus = ShipmentStatus.Yolda;
                shipment.AssignedVehicleId = vehicle.Id;
                shipment.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Konsolidasyon planı oluşturuldu: Plan {PlanId}, " +
                "Araç {PlateNumber}, {Count} kargo, " +
                "Doluluk {Rate:F1}%, Tasarruf {Saving:F2} TL",
                plan.Id, vehicle.PlateNumber, usedCapacity,
                occupancyRate, fuelSaving);
        }
    }
}