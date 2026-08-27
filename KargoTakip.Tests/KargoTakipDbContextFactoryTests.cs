using KargoTakip.Infrastructure.Data;
using Xunit;

namespace KargoTakip.Tests
{
    public class KargoTakipDbContextFactoryTests
    {
        [Fact]
        public void YerelVarsayilan_SifreIcermez()
        {
            // Baglanti dizesi sifresiyle birlikte koda gomuluydu ve git
            // gecmisine islenmisti (SonarCloud S6703, Blocker).
            Assert.DoesNotContain(
                "Password",
                KargoTakipDbContextFactory.YerelVarsayilan,
                System.StringComparison.OrdinalIgnoreCase);

            Assert.Contains("Trusted_Connection=True", KargoTakipDbContextFactory.YerelVarsayilan);
        }

        [Fact]
        public void OrtamDegiskeni_Tanimliysa_OncelikliKullanilir()
        {
            var beklenen = "Server=ornek;Database=Test;Trusted_Connection=True;";
            Environment.SetEnvironmentVariable(
                KargoTakipDbContextFactory.OrtamDegiskeni, beklenen);

            try
            {
                Assert.Equal(beklenen, KargoTakipDbContextFactory.BaglantiDizesiAl());
            }
            finally
            {
                Environment.SetEnvironmentVariable(
                    KargoTakipDbContextFactory.OrtamDegiskeni, null);
            }
        }

        [Fact]
        public void OrtamDegiskeni_YoksaVeyaBossa_YerelVarsayilanaDusulur()
        {
            foreach (var deger in new string?[] { null, "", "   " })
            {
                Environment.SetEnvironmentVariable(
                    KargoTakipDbContextFactory.OrtamDegiskeni, deger);

                Assert.Equal(
                    KargoTakipDbContextFactory.YerelVarsayilan,
                    KargoTakipDbContextFactory.BaglantiDizesiAl());
            }
        }
    }
}
