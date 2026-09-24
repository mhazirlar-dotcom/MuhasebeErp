using Accounting.Core.Domain.Constants;
using Accounting.Core.Domain.Entities;
using FluentValidation;

namespace Accounting.Core.Business.Validators.Master;

public sealed class PeriodValidator : AbstractValidator<Period>
{
    #region Constructor
    public PeriodValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("Firma zorunludur.");

        RuleFor(x => x.Year)
            .GreaterThan(0).WithMessage("Yıl zorunludur.");

        RuleFor(x => x.MonthNumber)
            .InclusiveBetween(1 , 12).WithMessage("Ay 1-12 arasında olmalıdır.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");

        RuleFor(x => x.FirmClass)
            .Must(fc => fc == FirmClasses.Class1 || fc == FirmClasses.Class2)
            .WithMessage("Firma sınıfı 1 veya 2 olmalıdır.");

        // 1. Sınıf alanları - sadece FirmClass=1 olduğunda zorunlu
        When(x => x.FirmClass == FirmClasses.Class1 , () =>
        {
            RuleFor(x => x.GrossWage)
                .GreaterThanOrEqualTo(0m).WithMessage("Brüt ücret negatif olamaz.");

            RuleFor(x => x.NetWage)
                .GreaterThanOrEqualTo(0m).WithMessage("Net ücret negatif olamaz.");

            RuleFor(x => x.VatAmount)
                .GreaterThanOrEqualTo(0m).WithMessage("KDV tutarı negatif olamaz.");

            RuleFor(x => x.StorageAmount)
                .GreaterThanOrEqualTo(0m).WithMessage("Depolama tutarı negatif olamaz.");

            RuleFor(x => x.JournalStartNumber)
                .GreaterThanOrEqualTo(0).WithMessage("Yevmiye başlangıç no negatif olamaz.");
        });

        // 2. Sınıf alanları - sadece FirmClass=2 olduğunda zorunlu
        When(x => x.FirmClass == FirmClasses.Class2 , () =>
        {
            RuleFor(x => x.IncomeStartNumber)
                .GreaterThanOrEqualTo(0).WithMessage("Gelir başlangıç no negatif olamaz.");

            RuleFor(x => x.ExpenseStartNumber)
                .GreaterThanOrEqualTo(0).WithMessage("Gider başlangıç no negatif olamaz.");

            RuleFor(x => x.CarryoverVat)
                .GreaterThanOrEqualTo(0m).WithMessage("Devreden KDV negatif olamaz.");
        });
    }
    #endregion Constructor
}