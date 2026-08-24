using OrderService.Services;
using Xunit;

namespace KargoTakip.Tests
{
    public class KodUreticiTests
    {
        [Fact]
        public void TakipKodu_BeklenenFormattadir()
        {
            var kod = KodUretici.TakipKodu();

            Assert.StartsWith("KRG-", kod);
            Assert.Equal("KRG-".Length + 10, kod.Length);
        }

        [Fact]
        public void TakipKodu_KarisabilecekKarakterIcermez()
        {
            // I/O/0/1 elle okunurken karistirildigi icin alfabede yok.
            for (int i = 0; i < 200; i++)
            {
                var govde = KodUretici.TakipKodu()["KRG-".Length..];
                Assert.DoesNotContain('I', govde);
                Assert.DoesNotContain('O', govde);
                Assert.DoesNotContain('0', govde);
                Assert.DoesNotContain('1', govde);
            }
        }

        [Fact]
        public void TakipKodu_UretimlerCakismaz()
        {
            // Eski uretim Ticks.Substring tabanliydi ve ayni kodu iki kargoya
            // verebiliyordu.
            var kodlar = new HashSet<string>();
            for (int i = 0; i < 5000; i++)
                Assert.True(kodlar.Add(KodUretici.TakipKodu()));
        }

        [Fact]
        public void TeslimatKodu_AltiHaneliOlur()
        {
            for (int i = 0; i < 500; i++)
            {
                var kod = KodUretici.TeslimatKodu();
                Assert.Equal(6, kod.Length);
                Assert.True(int.TryParse(kod, out var sayi));
                Assert.InRange(sayi, 100000, 999999);
            }
        }
    }
}
