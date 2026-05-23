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

            // Şehirler arası mesafeler
            var cities = await _context.Cities.ToListAsync();
            var cityDict = cities.ToDictionary(c => c.Name, c => c.Id);

            // İstanbul'un id'si zaten 1, diğerleri seed ile eklendi
            // Tüm şehirlerin id'lerini isimden bulacağız
            var distances = new List<CityDistance>();

            void AddDistance(string from, string to, int km, decimal hours)
            {
                if (!cityDict.ContainsKey(from) || !cityDict.ContainsKey(to)) return;
                distances.Add(new CityDistance
                {
                    FromCityId = cityDict[from],
                    ToCityId = cityDict[to],
                    DistanceKm = km,
                    EstimatedHours = hours
                });
                distances.Add(new CityDistance
                {
                    FromCityId = cityDict[to],
                    ToCityId = cityDict[from],
                    DistanceKm = km,
                    EstimatedHours = hours
                });
            }

            // İstanbul çıkışlı
            AddDistance("İstanbul", "Ankara", 452, 4.5m);
            AddDistance("İstanbul", "İzmir", 479, 5.0m);
            AddDistance("İstanbul", "Bursa", 154, 2.0m);
            AddDistance("İstanbul", "Adana", 939, 9.5m);
            AddDistance("İstanbul", "Mersin", 971, 10.0m);
            AddDistance("İstanbul", "Antalya", 725, 7.5m);
            AddDistance("İstanbul", "Konya", 660, 6.5m);
            AddDistance("İstanbul", "Gaziantep", 1130, 11.5m);
            AddDistance("İstanbul", "Kayseri", 770, 8.0m);
            AddDistance("İstanbul", "Samsun", 732, 7.5m);
            AddDistance("İstanbul", "Trabzon", 1100, 11.0m);
            AddDistance("İstanbul", "Erzurum", 1275, 13.0m);
            AddDistance("İstanbul", "Diyarbakır", 1340, 13.5m);
            AddDistance("İstanbul", "Malatya", 1025, 10.5m);

            // Ankara çıkışlı
            AddDistance("Ankara", "İzmir", 599, 6.0m);
            AddDistance("Ankara", "Adana", 491, 5.0m);
            AddDistance("Ankara", "Mersin", 523, 5.5m);
            AddDistance("Ankara", "Antalya", 480, 5.0m);
            AddDistance("Ankara", "Konya", 261, 2.5m);
            AddDistance("Ankara", "Gaziantep", 705, 7.0m);
            AddDistance("Ankara", "Kayseri", 320, 3.0m);
            AddDistance("Ankara", "Samsun", 420, 4.5m);
            AddDistance("Ankara", "Trabzon", 780, 8.0m);
            AddDistance("Ankara", "Erzurum", 895, 9.0m);
            AddDistance("Ankara", "Diyarbakır", 945, 9.5m);
            AddDistance("Ankara", "Malatya", 580, 6.0m);
            AddDistance("Ankara", "Bursa", 352, 3.5m);

            // İzmir çıkışlı
            AddDistance("İzmir", "Antalya", 488, 5.0m);
            AddDistance("İzmir", "Bursa", 375, 4.0m);
            AddDistance("İzmir", "Konya", 567, 6.0m);
            AddDistance("İzmir", "Adana", 818, 8.5m);
            AddDistance("İzmir", "Mersin", 850, 9.0m);
            AddDistance("İzmir", "Gaziantep", 1100, 11.0m);
            AddDistance("İzmir", "Kayseri", 830, 8.5m);

            // Adana çıkışlı
            AddDistance("Adana", "Mersin", 67, 0.8m);
            AddDistance("Adana", "Gaziantep", 220, 2.5m);
            AddDistance("Adana", "Konya", 330, 3.5m);
            AddDistance("Adana", "Antalya", 452, 5.0m);
            AddDistance("Adana", "Kayseri", 339, 3.5m);
            AddDistance("Adana", "Malatya", 358, 4.0m);
            AddDistance("Adana", "Diyarbakır", 460, 5.0m);

            // Mersin çıkışlı
            AddDistance("Mersin", "Gaziantep", 287, 3.0m);
            AddDistance("Mersin", "Konya", 296, 3.0m);
            AddDistance("Mersin", "Antalya", 418, 4.5m);
            AddDistance("Mersin", "Kayseri", 371, 4.0m);
            AddDistance("Mersin", "Malatya", 390, 4.0m);

            // Antalya çıkışlı
            AddDistance("Antalya", "Konya", 296, 3.0m);
            AddDistance("Antalya", "Kayseri", 545, 5.5m);
            AddDistance("Antalya", "Gaziantep", 665, 7.0m);

            // Konya çıkışlı
            AddDistance("Konya", "Kayseri", 247, 2.5m);
            AddDistance("Konya", "Gaziantep", 475, 5.0m);
            AddDistance("Konya", "Malatya", 445, 4.5m);
            AddDistance("Konya", "Diyarbakır", 695, 7.0m);

            // Gaziantep çıkışlı
            AddDistance("Gaziantep", "Diyarbakır", 325, 3.5m);
            AddDistance("Gaziantep", "Malatya", 310, 3.5m);
            AddDistance("Gaziantep", "Kayseri", 455, 5.0m);

            // Kayseri çıkışlı
            AddDistance("Kayseri", "Malatya", 215, 2.5m);
            AddDistance("Kayseri", "Samsun", 485, 5.0m);
            AddDistance("Kayseri", "Erzurum", 590, 6.0m);
            AddDistance("Kayseri", "Diyarbakır", 535, 5.5m);

            // Samsun çıkışlı
            AddDistance("Samsun", "Trabzon", 355, 4.0m);
            AddDistance("Samsun", "Erzurum", 595, 6.0m);

            // Trabzon çıkışlı
            AddDistance("Trabzon", "Erzurum", 325, 3.5m);

            // Erzurum çıkışlı
            AddDistance("Erzurum", "Diyarbakır", 440, 4.5m);
            AddDistance("Erzurum", "Malatya", 465, 5.0m);

            // Diyarbakır çıkışlı
            AddDistance("Diyarbakır", "Malatya", 260, 3.0m);

            // Bursa çıkışlı
            AddDistance("Bursa", "Antalya", 615, 6.5m);
            AddDistance("Bursa", "İzmir", 375, 4.0m);

            await _context.CityDistances.AddRangeAsync(distances);
            await _context.SaveChangesAsync();


            await _context.Vehicles.AddRangeAsync(vehicles);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Seed verisi başarıyla eklendi.",
                eklenenSehirler = 5,
                eklenenSubeler = 6,
                eklenenKullanicilar = users.Count,
                eklenenAraclar = vehicles.Count,
                eklenenMesafe = distances.Count
            });
        }

        [HttpPost("distances")]
        public async Task<IActionResult> SeedDistances()
        {
            if (await _context.CityDistances.AnyAsync())
                return BadRequest(new { message = "Mesafe verisi zaten mevcut." });

            var cities = await _context.Cities.ToListAsync();
            var cityDict = cities.ToDictionary(c => c.Name, c => c.Id);

            var distances = new List<CityDistance>();

            void AddDistance(string from, string to, int km, decimal hours)
            {
                if (!cityDict.ContainsKey(from) || !cityDict.ContainsKey(to)) return;
                distances.Add(new CityDistance
                {
                    FromCityId = cityDict[from],
                    ToCityId = cityDict[to],
                    DistanceKm = km,
                    EstimatedHours = hours
                });
                distances.Add(new CityDistance
                {
                    FromCityId = cityDict[to],
                    ToCityId = cityDict[from],
                    DistanceKm = km,
                    EstimatedHours = hours
                });
            }

            AddDistance("İstanbul", "Ankara", 452, 4.5m);
            AddDistance("İstanbul", "İzmir", 479, 5.0m);
            AddDistance("İstanbul", "Bursa", 154, 2.0m);
            AddDistance("İstanbul", "Adana", 939, 9.5m);
            AddDistance("İstanbul", "Mersin", 971, 10.0m);
            AddDistance("İstanbul", "Antalya", 725, 7.5m);
            AddDistance("İstanbul", "Konya", 660, 6.5m);
            AddDistance("İstanbul", "Gaziantep", 1130, 11.5m);
            AddDistance("İstanbul", "Kayseri", 770, 8.0m);
            AddDistance("İstanbul", "Samsun", 732, 7.5m);
            AddDistance("İstanbul", "Trabzon", 1100, 11.0m);
            AddDistance("İstanbul", "Erzurum", 1275, 13.0m);
            AddDistance("İstanbul", "Diyarbakır", 1340, 13.5m);
            AddDistance("İstanbul", "Malatya", 1025, 10.5m);
            AddDistance("Ankara", "İzmir", 599, 6.0m);
            AddDistance("Ankara", "Adana", 491, 5.0m);
            AddDistance("Ankara", "Mersin", 523, 5.5m);
            AddDistance("Ankara", "Antalya", 480, 5.0m);
            AddDistance("Ankara", "Konya", 261, 2.5m);
            AddDistance("Ankara", "Gaziantep", 705, 7.0m);
            AddDistance("Ankara", "Kayseri", 320, 3.0m);
            AddDistance("Ankara", "Samsun", 420, 4.5m);
            AddDistance("Ankara", "Trabzon", 780, 8.0m);
            AddDistance("Ankara", "Erzurum", 895, 9.0m);
            AddDistance("Ankara", "Diyarbakır", 945, 9.5m);
            AddDistance("Ankara", "Malatya", 580, 6.0m);
            AddDistance("Ankara", "Bursa", 352, 3.5m);
            AddDistance("İzmir", "Antalya", 488, 5.0m);
            AddDistance("İzmir", "Bursa", 375, 4.0m);
            AddDistance("İzmir", "Konya", 567, 6.0m);
            AddDistance("İzmir", "Adana", 818, 8.5m);
            AddDistance("İzmir", "Mersin", 850, 9.0m);
            AddDistance("İzmir", "Gaziantep", 1100, 11.0m);
            AddDistance("İzmir", "Kayseri", 830, 8.5m);
            AddDistance("Adana", "Mersin", 67, 0.8m);
            AddDistance("Adana", "Gaziantep", 220, 2.5m);
            AddDistance("Adana", "Konya", 330, 3.5m);
            AddDistance("Adana", "Antalya", 452, 5.0m);
            AddDistance("Adana", "Kayseri", 339, 3.5m);
            AddDistance("Adana", "Malatya", 358, 4.0m);
            AddDistance("Adana", "Diyarbakır", 460, 5.0m);
            AddDistance("Mersin", "Gaziantep", 287, 3.0m);
            AddDistance("Mersin", "Konya", 296, 3.0m);
            AddDistance("Mersin", "Antalya", 418, 4.5m);
            AddDistance("Mersin", "Kayseri", 371, 4.0m);
            AddDistance("Mersin", "Malatya", 390, 4.0m);
            AddDistance("Antalya", "Konya", 296, 3.0m);
            AddDistance("Antalya", "Kayseri", 545, 5.5m);
            AddDistance("Antalya", "Gaziantep", 665, 7.0m);
            AddDistance("Konya", "Kayseri", 247, 2.5m);
            AddDistance("Konya", "Gaziantep", 475, 5.0m);
            AddDistance("Konya", "Malatya", 445, 4.5m);
            AddDistance("Konya", "Diyarbakır", 695, 7.0m);
            AddDistance("Gaziantep", "Diyarbakır", 325, 3.5m);
            AddDistance("Gaziantep", "Malatya", 310, 3.5m);
            AddDistance("Gaziantep", "Kayseri", 455, 5.0m);
            AddDistance("Kayseri", "Malatya", 215, 2.5m);
            AddDistance("Kayseri", "Samsun", 485, 5.0m);
            AddDistance("Kayseri", "Erzurum", 590, 6.0m);
            AddDistance("Kayseri", "Diyarbakır", 535, 5.5m);
            AddDistance("Samsun", "Trabzon", 355, 4.0m);
            AddDistance("Samsun", "Erzurum", 595, 6.0m);
            AddDistance("Trabzon", "Erzurum", 325, 3.5m);
            AddDistance("Erzurum", "Diyarbakır", 440, 4.5m);
            AddDistance("Erzurum", "Malatya", 465, 5.0m);
            AddDistance("Diyarbakır", "Malatya", 260, 3.0m);
            AddDistance("Bursa", "Antalya", 615, 6.5m);
            AddDistance("Bursa", "İzmir", 375, 4.0m);

            await _context.CityDistances.AddRangeAsync(distances);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Mesafe verisi başarıyla eklendi.",
                eklenenMesafe = distances.Count
            });
        }
    }
}