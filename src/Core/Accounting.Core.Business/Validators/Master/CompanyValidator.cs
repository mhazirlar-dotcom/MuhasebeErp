using Accounting.Core.Domain.Entities;
using FluentValidation;

namespace Accounting.Core.Business.Validators.Master;

public sealed class CompanyValidator : AbstractValidator<Company>
{
    #region Constructor
    public CompanyValidator()
    {
        RuleFor(x => x.ShortName)
            .NotEmpty().WithMessage("Kısa ad zorunludur.")
            .MaximumLength(64).WithMessage("Kısa ad en fazla 64 karakter olabilir.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Uzun ad zorunludur.")
            .MaximumLength(256).WithMessage("Uzun ad en fazla 256 karakter olabilir.");

        RuleFor(x => x.TaxNumber)
            .NotEmpty().WithMessage("Vergi No zorunludur.")
            .MinimumLength(10).WithMessage("Vergi No en az 10 karakter olabilir.")
            .MaximumLength(10).WithMessage("Vergi No en fazla 10 karakter olabilir.");

        RuleFor(x => x.IdentityNumber)
            .MinimumLength(11).WithMessage("TC Kimlik No en az 11 karakter olabilir.")
            .MaximumLength(11).WithMessage("TC Kimlik No en fazla 11 karakter olabilir.")
            .When(x => !string.IsNullOrWhiteSpace(x.IdentityNumber));

        RuleFor(x => x.Description)
            .MaximumLength(1024).WithMessage("Açıklama en fazla 1024 karakter olabilir.");

        RuleFor(x => x.FoundationDate)
            .Must(date => date != DateTime.MinValue)
            .WithMessage("Kuruluş tarihi zorunludur.");

        RuleFor(x => x.PayrollCutoffDate)
            .Must((company , cutoffDate) => cutoffDate == DateTime.MinValue || cutoffDate >= company.FoundationDate)
            .WithMessage("Bordro kesilme tarihi kuruluş tarihinden önce olamaz.")
            .When(x => x.FoundationDate != DateTime.MinValue);
    }
    #endregion Constructor
}