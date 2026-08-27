using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KargoTakip.Infrastructure.Data
{
    /// <summary>
    /// "dotnet ef" komutlarinin tasarim zamaninda kullandigi factory.
    ///
    /// Onceden baglanti dizesi sifresiyle birlikte koda gomuluydu ve
    /// git gecmisine islenmisti. Deger artik yalnizca ortam degiskeninden
    /// gelir; tanimli degilse sifre gerektirmeyen yerel varsayilana dusulur.
    ///
    /// Kullanim:
    ///   ConnectionStrings__DefaultConnection="..." dotnet ef migrations add X ...
    /// </summary>
    public class KargoTakipDbContextFactory
        : IDesignTimeDbContextFactory<KargoTakipDbContext>
    {
        public const string OrtamDegiskeni = "ConnectionStrings__DefaultConnection";

        /// <summary>
        /// Windows uzerinde yerel SQL Server Express; sifre icermez,
        /// Windows kimlik dogrulamasi kullanir.
        /// </summary>
        public const string YerelVarsayilan =
            "Server=.\\SQLEXPRESS;Database=KargoTakipDB;" +
            "Trusted_Connection=True;TrustServerCertificate=True;";

        public KargoTakipDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<KargoTakipDbContext>();
            optionsBuilder.UseSqlServer(BaglantiDizesiAl());

            return new KargoTakipDbContext(optionsBuilder.Options);
        }

        public static string BaglantiDizesiAl()
        {
            var ortamdan = Environment.GetEnvironmentVariable(OrtamDegiskeni);

            return string.IsNullOrWhiteSpace(ortamdan) ? YerelVarsayilan : ortamdan;
        }
    }
}
