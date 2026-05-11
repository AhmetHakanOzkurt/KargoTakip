using KargoTakip.Infrastructure.Data;
using KargoTakip.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/seed")]
    public class SeedController : ControllerBase
    {
        private readonly KargoTakipDbContext _context;

        public SeedController(KargoTakipDbContext context)
        {
            _context = context;
        }

        [HttpPost("all")]
        public async Task<IActionResult> SeedAll()
        {
            // Zaten veri varsa tekrar ekleme
            if (await _context.Cities.CountAsync() > 1)
                return BadRequest(new { message = "Veri zaten mevcut." });

            // Şehirler
            var ankara = new City { Name = "Ankara", Region = "İç Anadolu" };
            var adana = new City { Name = "Adana", Region = "Akdeniz" };
            var izmir = new City { Name = "İzmir", Region = "Ege" };
            var bursa = new City { Name = "Bursa", Region = "Marmara" };
            var mersin = new City { Name = "Mersin", Region = "Akdeniz" };

            await _context.Cities.AddRangeAsync(ankara, adana, izmir, bursa, mersin);
            await _context.SaveChangesAsync();

            // Şubeler
            var istanbulB = new Branch
            {
                Name = "İstanbul B Şubesi",
                CityId = 1,
                Address = "Şişli, İstanbul",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var istanbulC = new Branch
            {
                Name = "İstanbul C Şubesi",
                CityId = 1,
                Address = "Üsküdar, İstanbul",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var ankaraA = new Branch
            {
                Name = "Ankara A Şubesi",
                CityId = ankara.Id,
                Address = "Çankaya, Ankara",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var ankaraB = new Branch
            {
                Name = "Ankara B Şubesi",
                CityId = ankara.Id,
                Address = "Keçiören, Ankara",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var adanaA = new Branch
            {
                Name = "Adana A Şubesi",
                CityId = adana.Id,
                Address = "Seyhan, Adana",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var izmirA = new Branch
            {
                Name = "İzmir A Şubesi",
                CityId = izmir.Id,
                Address = "Konak, İzmir",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Branches.AddRangeAsync(
                istanbulB, istanbulC, ankaraA, ankaraB, adanaA, izmirA);
            await _context.SaveChangesAsync();

            // Kullanıcılar
            var users = new List<User>
            {
                new User
                {
                    Username = "istanbul_b_manager",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"),
                    FullName = "İstanbul B Şube Müdürü",
                    Role = "BranchManager",
                    BranchId = istanbulB.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new User
                {
                    Username = "istanbul_c_manager",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"),
                    FullName = "İstanbul C Şube Müdürü",
                    Role = "BranchManager",
                    BranchId = istanbulC.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new User
                {
                    Username = "ankara_a_manager",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"),
                    FullName = "Ankara A Şube Müdürü",
                    Role = "BranchManager",
                    BranchId = ankaraA.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new User
                {
                    Username = "adana_a_manager",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"),
                    FullName = "Adana A Şube Müdürü",
                    Role = "BranchManager",
                    BranchId = adanaA.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new User
                {
                    Username = "istanbul_a_staff",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("staff123"),
                    FullName = "İstanbul A Personel",
                    Role = "Staff",
                    BranchId = 1,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            };

            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            // Araçlar
            var vehicles = new List<Vehicle>
            {
                new Vehicle
                {
                    PlateNumber = "34 DEF 002",
                    Capacity = 20,
                    CurrentLoad = 0,
                    VehicleTypeId = 2,
                    BranchId = istanbulB.Id,
                    CityId = 1,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Vehicle
                {
                    PlateNumber = "34 GHI 003",
                    Capacity = 100,
                    CurrentLoad = 0,
                    VehicleTypeId = 3,
                    BranchId = istanbulC.Id,
                    CityId = 1,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Vehicle
                {
                    PlateNumber = "06 ABC 001",
                    Capacity = 20,
                    CurrentLoad = 0,
                    VehicleTypeId = 2,
                    BranchId = ankaraA.Id,
                    CityId = ankara.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Vehicle
                {
                    PlateNumber = "06 DEF 002",
                    Capacity = 100,
                    CurrentLoad = 0,
                    VehicleTypeId = 3,
                    BranchId = ankaraB.Id,
                    CityId = ankara.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Vehicle
                {
                    PlateNumber = "01 ABC 001",
                    Capacity = 100,
                    CurrentLoad = 0,
                    VehicleTypeId = 3,
                    BranchId = adanaA.Id,
                    CityId = adana.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Vehicle
                {
                    PlateNumber = "35 ABC 001",
                    Capacity = 20,
                    CurrentLoad = 0,
                    VehicleTypeId = 2,
                    BranchId = izmirA.Id,
                    CityId = izmir.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _context.Vehicles.AddRangeAsync(vehicles);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Seed verisi başarıyla eklendi.",
                eklenenSehirler = 5,
                eklenenSubeler = 6,
                eklenenKullanicilar = users.Count,
                eklenenAraclar = vehicles.Count
            });
        }
    }
}