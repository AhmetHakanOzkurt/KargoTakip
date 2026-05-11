using KargoTakip.Infrastructure.Data;
using KargoTakip.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VehicleService.Controllers
{
    [ApiController]
    [Route("api/vehicles")]
    [Authorize]
    public class VehicleController : ControllerBase
    {
        private readonly KargoTakipDbContext _context;

        public VehicleController(KargoTakipDbContext context)
        {
            _context = context;
        }

        // Tüm araçları listele
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var vehicles = await _context.Vehicles
                .Include(v => v.VehicleType)
                .Include(v => v.Branch)
                .Include(v => v.City)
                .Select(v => new
                {
                    v.Id,
                    v.PlateNumber,
                    v.Capacity,
                    v.CurrentLoad,
                    OccupancyRate = v.Capacity > 0
                        ? (int)Math.Round((double)v.CurrentLoad / v.Capacity * 100)
                        : 0,
                    v.IsAvailable,
                    VehicleType = v.VehicleType.Name,
                    RouteType = v.VehicleType.RouteType,
                    Branch = v.Branch.Name,
                    City = v.City.Name,
                    v.CreatedAt
                })
                .ToListAsync();

            return Ok(vehicles);
        }

        // Müsait araçları listele
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable([FromQuery] int? cityId)
        {
            var query = _context.Vehicles
                .Include(v => v.VehicleType)
                .Include(v => v.Branch)
                .Include(v => v.City)
                .Where(v => v.IsAvailable && v.CurrentLoad < v.Capacity);

            if (cityId.HasValue)
                query = query.Where(v => v.CityId == cityId.Value);

            var vehicles = await query
                .Select(v => new
                {
                    v.Id,
                    v.PlateNumber,
                    v.Capacity,
                    v.CurrentLoad,
                    RemainingCapacity = v.Capacity - v.CurrentLoad,
                    OccupancyRate = v.Capacity > 0
                        ? (int)Math.Round((double)v.CurrentLoad / v.Capacity * 100)
                        : 0,
                    VehicleType = v.VehicleType.Name,
                    RouteType = v.VehicleType.RouteType,
                    Branch = v.Branch.Name,
                    City = v.City.Name
                })
                .OrderBy(v => v.OccupancyRate)
                .ToListAsync();

            return Ok(vehicles);
        }

        // Tek araç getir
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.VehicleType)
                .Include(v => v.Branch)
                .Include(v => v.City)
                .Where(v => v.Id == id)
                .Select(v => new
                {
                    v.Id,
                    v.PlateNumber,
                    v.Capacity,
                    v.CurrentLoad,
                    RemainingCapacity = v.Capacity - v.CurrentLoad,
                    OccupancyRate = v.Capacity > 0
                        ? (int)Math.Round((double)v.CurrentLoad / v.Capacity * 100)
                        : 0,
                    v.IsAvailable,
                    VehicleType = v.VehicleType.Name,
                    RouteType = v.VehicleType.RouteType,
                    Branch = v.Branch.Name,
                    City = v.City.Name,
                    v.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (vehicle == null)
                return NotFound(new { message = "Araç bulunamadı." });

            return Ok(vehicle);
        }

        // Yeni araç ekle
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVehicleRequest request)
        {
            var branch = await _context.Branches.FindAsync(request.BranchId);
            if (branch == null)
                return BadRequest(new { message = "Şube bulunamadı." });

            var city = await _context.Cities.FindAsync(request.CityId);
            if (city == null)
                return BadRequest(new { message = "Şehir bulunamadı." });

            var vehicleType = await _context.VehicleTypes.FindAsync(request.VehicleTypeId);
            if (vehicleType == null)
                return BadRequest(new { message = "Araç tipi bulunamadı." });

            var vehicle = new Vehicle
            {
                PlateNumber = request.PlateNumber,
                Capacity = request.Capacity,
                CurrentLoad = 0,
                VehicleTypeId = request.VehicleTypeId,
                BranchId = request.BranchId,
                CityId = request.CityId,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, new
            {
                vehicle.Id,
                vehicle.PlateNumber,
                vehicle.Capacity,
                message = "Araç başarıyla eklendi."
            });
        }

        // Tüm şubelerin doluluk özeti
        [HttpGet("branch-summary")]
        public async Task<IActionResult> GetBranchSummary()
        {
            var summary = await _context.Branches
                .Where(b => b.IsActive)
                .Select(b => new
                {
                    SubeId = b.Id,
                    SubeAdi = b.Name,
                    Sehir = b.City.Name,
                    ToplamArac = b.Vehicles.Count(),
                    MüsaitArac = b.Vehicles.Count(v => v.IsAvailable),
                    MesgulArac = b.Vehicles.Count(v => !v.IsAvailable),
                    ToplamKapasite = b.Vehicles.Sum(v => v.Capacity),
                    ToplamYuk = b.Vehicles.Sum(v => v.CurrentLoad),
                    DolulukOrani = b.Vehicles.Any()
                        ? (int)Math.Round(
                            (double)b.Vehicles.Sum(v => v.CurrentLoad) /
                            b.Vehicles.Sum(v => v.Capacity) * 100)
                        : 0,
                    AktifKargo = b.Shipments
                        .Count(s => s.CurrentStatus != "Teslim Edildi")
                })
                .OrderBy(b => b.SubeAdi)
                .ToListAsync();

            return Ok(summary);
        }

        // Belirli bir şubenin detaylı araç durumu
        [HttpGet("branch/{branchId}")]
        public async Task<IActionResult> GetByBranch(int branchId)
        {
            var branch = await _context.Branches.FindAsync(branchId);
            if (branch == null)
                return NotFound(new { message = "Şube bulunamadı." });

            var vehicles = await _context.Vehicles
                .Include(v => v.VehicleType)
                .Include(v => v.City)
                .Where(v => v.BranchId == branchId)
                .Select(v => new
                {
                    v.Id,
                    v.PlateNumber,
                    Tip = v.VehicleType.Name,
                    RotaTipi = v.VehicleType.RouteType,
                    v.Capacity,
                    v.CurrentLoad,
                    KalanKapasite = v.Capacity - v.CurrentLoad,
                    DolulukOrani = v.Capacity > 0
                        ? (int)Math.Round(
                            (double)v.CurrentLoad / v.Capacity * 100)
                        : 0,
                    v.IsAvailable,
                    BulunduguSehir = v.City.Name
                })
                .OrderBy(v => v.DolulukOrani)
                .ToListAsync();

            return Ok(new
            {
                subeId = branchId,
                subeAdi = branch.Name,
                araclar = vehicles
            });
        }

        // Araç ata — OrderService tarafından çağrılacak
        [AllowAnonymous]
        [HttpPost("assign")]
        public async Task<IActionResult> AssignVehicle([FromBody] AssignVehicleRequest request)
        {
            // Uygun araç bul: müsait, aynı şehir, yeterli kapasite
            var vehicle = await _context.Vehicles
                .Include(v => v.VehicleType)
                .Where(v =>
                    v.IsAvailable &&
                    v.CityId == request.CityId &&
                    (v.Capacity - v.CurrentLoad) >= request.RequiredCapacity)
                .OrderBy(v => v.CurrentLoad)
                .FirstOrDefaultAsync();

            if (vehicle == null)
                return NotFound(new { message = "Uygun araç bulunamadı." });

            vehicle.CurrentLoad += request.RequiredCapacity;
            if (vehicle.CurrentLoad >= vehicle.Capacity)
                vehicle.IsAvailable = false;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                vehicleId = vehicle.Id,
                plateNumber = vehicle.PlateNumber,
                message = "Araç başarıyla atandı."
            });
        }
    }

    public class CreateVehicleRequest
    {
        public string PlateNumber { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int VehicleTypeId { get; set; }
        public int BranchId { get; set; }
        public int CityId { get; set; }
    }

    public class AssignVehicleRequest
    {
        public int CityId { get; set; }
        public int RequiredCapacity { get; set; }
    }
}
