using Accounting.Shared.Dtos.Auth;
using FluentValidation;

namespace Accounting.Core.Business.Validators.Auth;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    #region Constructor
    public LoginRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Kullanıcı adı zorunludur.")
            .MaximumLength(64).WithMessage("Kullanıcı adı en fazla 64 karakter olabilir.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre zorunludur.");
    }
    #endregion Constructor
}