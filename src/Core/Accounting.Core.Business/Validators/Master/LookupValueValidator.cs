using Accounting.Core.Domain.Entities;
using FluentValidation;

namespace Accounting.Core.Business.Validators.Master;

public sealed class LookupValueValidator : AbstractValidator<LookupValue>
{
    #region Constructor
    public LookupValueValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Lookup tipi zorunludur.")
            .MaximumLength(64).WithMessage("Lookup tipi en fazla 64 karakter olabilir.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Lookup kodu zorunludur.")
            .MaximumLength(64).WithMessage("Lookup kodu en fazla 64 karakter olabilir.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Lookup adı zorunludur.");
    }
    #endregion Constructor
}