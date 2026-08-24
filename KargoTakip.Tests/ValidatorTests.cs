using OrderService.Controllers;
using OrderService.Validators;
using Xunit;

namespace KargoTakip.Tests
{
    public class UpdateStatusRequestValidatorTests
    {
        private readonly UpdateStatusRequestValidator _validator = new();

        [Theory]
        [InlineData("Hazırlanıyor")]
        [InlineData("Yolda")]
        [InlineData("Dağıtımda")]
        [InlineData("Teslim Edildi")]
        [InlineData("İptal")]
        public void GecerliDurumlar_KabulEdilir(string durum)
        {
            var sonuc = _validator.Validate(new UpdateStatusRequest { NewStatus = durum });

            Assert.True(sonuc.IsValid);
        }

        [Theory]
        [InlineData("asdf")]
        [InlineData("teslim edildi")]
        [InlineData("")]
        public void GecersizDurumlar_Reddedilir(string durum)
        {
            // Bu validator yazilmis ama controller'a hic baglanmamisti;
            // "asdf" gibi degerler dogrudan yazilabiliyordu.
            var sonuc = _validator.Validate(new UpdateStatusRequest { NewStatus = durum });

            Assert.False(sonuc.IsValid);
        }
    }

    public class CreateShipmentRequestValidatorTests
    {
        private readonly CreateShipmentRequestValidator _validator = new();

        private static CreateShipmentRequest GecerliIstek() => new()
        {
            SenderName = "Ahmet",
            ReceiverName = "Mehmet",
            ReceiverAddress = "Örnek Mah. 1. Sk. No:1",
            ReceiverCityId = 1,
            Weight = 5.5m,
            Priority = "Normal"
        };

        [Fact]
        public void GecerliIstek_KabulEdilir()
        {
            Assert.True(_validator.Validate(GecerliIstek()).IsValid);
        }

        [Fact]
        public void BranchId_Verilmezse_KabulEdilir()
        {
            // BranchId artik token'dan geliyor; istemci gondermek zorunda degil.
            var istek = GecerliIstek();
            istek.BranchId = null;

            Assert.True(_validator.Validate(istek).IsValid);
        }

        [Fact]
        public void BranchId_Sifirsa_Reddedilir()
        {
            var istek = GecerliIstek();
            istek.BranchId = 0;

            Assert.False(_validator.Validate(istek).IsValid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1001)]
        public void GecersizAgirlik_Reddedilir(int agirlik)
        {
            var istek = GecerliIstek();
            istek.Weight = agirlik;

            Assert.False(_validator.Validate(istek).IsValid);
        }

        [Fact]
        public void GecersizOncelik_Reddedilir()
        {
            var istek = GecerliIstek();
            istek.Priority = "Çok Acil";

            Assert.False(_validator.Validate(istek).IsValid);
        }
    }
}
