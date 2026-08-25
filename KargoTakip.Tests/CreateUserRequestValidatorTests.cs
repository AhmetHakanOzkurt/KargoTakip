using AuthService.Controllers;
using AuthService.Validators;
using Xunit;

namespace KargoTakip.Tests
{
    public class CreateUserRequestValidatorTests
    {
        private readonly CreateUserRequestValidator _validator = new();

        private static CreateUserRequest GecerliIstek() => new()
        {
            Username = "ahmet.ozkurt",
            Password = "GucluSifre123",
            FullName = "Ahmet Özkurt",
            Role = "Staff",
            BranchId = 1
        };

        [Fact]
        public void GecerliIstek_KabulEdilir()
        {
            Assert.True(_validator.Validate(GecerliIstek()).IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData("kisa1")]          // 10 karakterden az
        [InlineData("sadeceharfler")]  // rakam yok
        [InlineData("1234567890")]     // harf yok
        public void ZayifSifre_Reddedilir(string sifre)
        {
            // Onceden CreateUser'da hicbir sifre dogrulamasi yoktu.
            var istek = GecerliIstek();
            istek.Password = sifre;

            Assert.False(_validator.Validate(istek).IsValid);
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("BranchManager")]
        [InlineData("Staff")]
        [InlineData("Courier")]
        public void GecerliRoller_KabulEdilir(string rol)
        {
            var istek = GecerliIstek();
            istek.Role = rol;

            Assert.True(_validator.Validate(istek).IsValid);
        }

        [Theory]
        [InlineData("SuperAdmin")]
        [InlineData("admin")]
        [InlineData("")]
        public void GecersizRol_Reddedilir(string rol)
        {
            // Role serbest string'di; istenilen deger yazilabiliyordu.
            var istek = GecerliIstek();
            istek.Role = rol;

            Assert.False(_validator.Validate(istek).IsValid);
        }

        [Theory]
        [InlineData("ab")]
        [InlineData("bosluk iceren")]
        [InlineData("ozel!karakter")]
        public void GecersizKullaniciAdi_Reddedilir(string kullaniciAdi)
        {
            var istek = GecerliIstek();
            istek.Username = kullaniciAdi;

            Assert.False(_validator.Validate(istek).IsValid);
        }

        [Fact]
        public void SubeSecilmezse_Reddedilir()
        {
            var istek = GecerliIstek();
            istek.BranchId = 0;

            Assert.False(_validator.Validate(istek).IsValid);
        }
    }
}
