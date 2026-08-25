using KargoTakip.ServiceDefaults;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace KargoTakip.Tests
{
    public class YerelZamanTests
    {
        private static YerelZaman Olustur(string? saatDilimi = null)
        {
            var ayarlar = new Dictionary<string, string?>();
            if (saatDilimi is not null)
                ayarlar["App:TimeZone"] = saatDilimi;

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(ayarlar)
                .Build();

            return new YerelZaman(configuration);
        }

        [Fact]
        public void GunBaslangici_UtcGeceYarisiyla_AyniDegildir()
        {
            // Turkiye UTC+3: yerel gun UTC 21:00'de baslar. Onceden
            // DateTime.UtcNow.Date kullanildigi icin gunun ilk 3 saatinde
            // olusan kayitlar bir onceki gune dusuyordu.
            var zaman = Olustur();

            var baslangic = zaman.BugunBaslangicUtc();

            Assert.Equal(21, baslangic.Hour);
            Assert.Equal(0, baslangic.Minute);
        }

        [Fact]
        public void GunAraligi_TamYirmiDortSaattir()
        {
            var zaman = Olustur();

            var fark = zaman.BugunBitisUtc() - zaman.BugunBaslangicUtc();

            Assert.Equal(TimeSpan.FromDays(1), fark);
        }

        [Fact]
        public void SimdikiZaman_UtcdenUcSaatIleridedir()
        {
            var zaman = Olustur();

            var fark = zaman.Simdi - DateTime.UtcNow;

            Assert.InRange(fark.TotalHours, 2.9, 3.1);
        }

        [Fact]
        public void YerelTarih_IcindeBulunulanAraliktadir()
        {
            var zaman = Olustur();

            var baslangic = zaman.BugunBaslangicUtc();
            var simdiUtc = DateTime.UtcNow;

            Assert.True(simdiUtc >= baslangic);
            Assert.True(simdiUtc < zaman.BugunBitisUtc());
        }

        [Fact]
        public void GecersizSaatDilimi_ServisiDusurmez()
        {
            // Saat dilimi verisi eksik bir imajda UTC+3 sabitine dusulur.
            var zaman = Olustur("Boyle/BirSaatDilimiYok");

            var fark = zaman.Simdi - DateTime.UtcNow;

            Assert.InRange(fark.TotalHours, 2.9, 3.1);
        }
    }

    public class ConsolidationPlanStatusTests
    {
        [Theory]
        [InlineData("Tamamlandı", true)]
        [InlineData("İptal", true)]
        [InlineData("Planlandı", false)]
        [InlineData("Yolda", false)]
        [InlineData(null, false)]
        public void TerminalMi_DogruSonucVerir(string? durum, bool beklenen)
        {
            Assert.Equal(
                beklenen,
                KargoTakip.Infrastructure.Models.ConsolidationPlanStatus.TerminalMi(durum));
        }
    }
}
