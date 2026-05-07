using FluentValidation;
using OrderService.Controllers;

namespace OrderService.Validators
{
    public class CreateShipmentRequestValidator
        : AbstractValidator<CreateShipmentRequest>
    {
        public CreateShipmentRequestValidator()
        {
            RuleFor(x => x.SenderName)
                .NotEmpty().WithMessage("Gönderici adı boş olamaz.")
                .MaximumLength(100).WithMessage("Gönderici adı en fazla 100 karakter olabilir.");

            RuleFor(x => x.ReceiverName)
                .NotEmpty().WithMessage("Alıcı adı boş olamaz.")
                .MaximumLength(100).WithMessage("Alıcı adı en fazla 100 karakter olabilir.");

            RuleFor(x => x.ReceiverAddress)
                .NotEmpty().WithMessage("Teslimat adresi boş olamaz.")
                .MaximumLength(255).WithMessage("Adres en fazla 255 karakter olabilir.");

            RuleFor(x => x.ReceiverCityId)
                .GreaterThan(0).WithMessage("Geçerli bir şehir seçilmeli.");

            RuleFor(x => x.Weight)
                .GreaterThan(0).WithMessage("Ağırlık sıfırdan büyük olmalı.")
                .LessThanOrEqualTo(1000).WithMessage("Ağırlık 1000 kg'dan fazla olamaz.");

            RuleFor(x => x.Priority)
                .Must(p => p == null ||
                    new[] { "Normal", "Acil", "Express" }.Contains(p))
                .WithMessage("Öncelik Normal, Acil veya Express olabilir.");

            RuleFor(x => x.BranchId)
                .GreaterThan(0).WithMessage("Geçerli bir şube seçilmeli.");

            RuleFor(x => x.CreatedByUserId)
                .GreaterThan(0).WithMessage("Geçerli bir kullanıcı belirtilmeli.");
        }
    }

    public class UpdateStatusRequestValidator
        : AbstractValidator<UpdateStatusRequest>
    {
        private static readonly string[] GecerliDurumlar =
        {
            "Hazırlanıyor", "Yolda", "Dağıtımda", "Teslim Edildi", "İptal"
        };

        public UpdateStatusRequestValidator()
        {
            RuleFor(x => x.NewStatus)
                .NotEmpty().WithMessage("Yeni durum boş olamaz.")
                .Must(s => GecerliDurumlar.Contains(s))
                .WithMessage($"Geçerli durumlar: {string.Join(", ", GecerliDurumlar)}");

            RuleFor(x => x.ChangedByUserId)
                .GreaterThan(0).WithMessage("Geçerli bir kullanıcı belirtilmeli.");
        }
    }
}