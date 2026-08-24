using ConsolidationService.Services;
using Xunit;

namespace KargoTakip.Tests
{
    public class KonsolidasyonHesapTests
    {
        [Fact]
        public void YakitTasarrufu_TekKargoIcin_SifirDoner()
        {
            // Tek kargo konsolide edilmis sayilmaz, tasarruf yoktur.
            Assert.Equal(0m, KonsolidasyonHesap.YakitTasarrufu(450m, 1));
        }

        [Fact]
        public void YakitTasarrufu_KargoSayisiArttikca_Artar()
        {
            // Eski formul bunun tersini yapiyordu: arac bosaldikca tasarruf
            // yukseliyordu. Regresyonu yakalamak icin yon testi.
            var az = KonsolidasyonHesap.YakitTasarrufu(450m, 3);
            var cok = KonsolidasyonHesap.YakitTasarrufu(450m, 9);

            Assert.True(cok > az);
        }

        [Fact]
        public void YakitTasarrufu_YapilmayanSeferSayisiylaOrantilidir()
        {
            // 450 km -> 450/100 * 35 litre * 35 TL = 5512.5 TL / sefer
            // 5 kargo -> 4 sefer tasarrufu
            var beklenen = 4 * (450m / 100m * 35m * 35m);

            Assert.Equal(beklenen, KonsolidasyonHesap.YakitTasarrufu(450m, 5));
        }

        [Fact]
        public void YakitTasarrufu_MesafeBilinmiyorsa_SifirDoner()
        {
            Assert.Equal(0m, KonsolidasyonHesap.YakitTasarrufu(0m, 10));
        }

        [Theory]
        [InlineData(5, 10, 50)]
        [InlineData(10, 10, 100)]
        [InlineData(0, 10, 0)]
        public void DolulukOrani_DogruHesaplanir(int kullanilan, int kapasite, int beklenen)
        {
            Assert.Equal(beklenen, KonsolidasyonHesap.DolulukOrani(kullanilan, kapasite));
        }

        [Fact]
        public void DolulukOrani_KapasiteAsilsaBile_Yuzu_Gecmez()
        {
            // precision(5,2) kolonu 100'un uzerini tasirdi.
            Assert.Equal(100m, KonsolidasyonHesap.DolulukOrani(25, 10));
        }

        [Fact]
        public void DolulukOrani_KapasiteSifirsa_SifirDoner()
        {
            Assert.Equal(0m, KonsolidasyonHesap.DolulukOrani(5, 0));
        }
    }
}
