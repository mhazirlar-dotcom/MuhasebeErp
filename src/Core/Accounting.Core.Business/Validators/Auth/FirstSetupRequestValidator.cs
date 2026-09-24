using Accounting.Shared.Dtos.Auth;
using FluentValidation;

namespace Accounting.Core.Business.Validators.Auth;

public sealed class FirstSetupRequestValidator : AbstractValidator<FirstSetupRequest>
{
    #region Constructor
    public FirstSetupRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Kullanıcı adı zorunludur.")
            .MaximumLength(64).WithMessage("Kullanıcı adı en fazla 64 karakter olabilir.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ad soyad zorunludur.")
            .MaximumLength(128).WithMessage("Ad soyad en fazla 128 karakter olabilir.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta zorunludur.")
            .MaximumLength(256).WithMessage("E-posta en fazla 256 karakter olabilir.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi girin.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre zorunludur.")
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.");
    }
    #endregion Constructor
}