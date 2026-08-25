using AuthService.Controllers;
using FluentValidation;

namespace AuthService.Validators
{
    /// <summary>
    /// CreateUser ucunda hicbir dogrulama yoktu: bos sifre kabul ediliyor,
    /// Role alanina serbest metin yazilabiliyordu.
    /// </summary>
    public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
    {
        public static readonly string[] GecerliRoller =
        {
            "Admin", "BranchManager", "Staff", "Courier"
        };

        public CreateUserRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Kullanıcı adı boş olamaz.")
                .MinimumLength(3).WithMessage("Kullanıcı adı en az 3 karakter olmalı.")
                .MaximumLength(50).WithMessage("Kullanıcı adı en fazla 50 karakter olabilir.")
                .Matches("^[a-zA-Z0-9._-]+$")
                .WithMessage("Kullanıcı adı yalnızca harf, rakam, nokta, alt çizgi ve tire içerebilir.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre boş olamaz.")
                .MinimumLength(10).WithMessage("Şifre en az 10 karakter olmalı.")
                .MaximumLength(128).WithMessage("Şifre en fazla 128 karakter olabilir.")
                .Matches("[A-Za-z]").WithMessage("Şifre en az bir harf içermeli.")
                .Matches("[0-9]").WithMessage("Şifre en az bir rakam içermeli.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Ad soyad boş olamaz.")
                .MaximumLength(100).WithMessage("Ad soyad en fazla 100 karakter olabilir.");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Rol boş olamaz.")
                .Must(r => GecerliRoller.Contains(r))
                .WithMessage($"Geçerli roller: {string.Join(", ", GecerliRoller)}");

            RuleFor(x => x.BranchId)
                .GreaterThan(0).WithMessage("Geçerli bir şube seçilmeli.");
        }
    }
}
