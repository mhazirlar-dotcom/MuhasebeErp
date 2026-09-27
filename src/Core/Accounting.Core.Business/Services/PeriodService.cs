using Accounting.Core.Business.Interfaces.Repositories;
using Accounting.Core.Business.Interfaces.Services;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Results;
using FluentValidation;

namespace Accounting.Core.Business.Services;

public sealed class PeriodService(IPeriodRepository periodRepository , ICompanyRepository companyRepository , IValidator<Period> validator) : ServiceBase<Period , IPeriodRepository>(periodRepository , validator), IPeriodService
{
    #region Operations
    public async Task<Result<IReadOnlyList<Period>>> GetByCompanyIdAsync(Guid companyId , CancellationToken cancellationToken = default)
    {
        if (companyId == Guid.Empty)
        {
            return Result<IReadOnlyList<Period>>.Failure("Firma Id zorunludur." , ResultStatus.ValidationError);
        }

        return await _repository.GetByCompanyIdAsync(companyId , cancellationToken);
    }

    public override async Task<Result<Period>> CreateAsync(Period period , CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Error>? errors = await ValidateAsync(period , cancellationToken);
        if (errors is not null)
        {
            return Result<Period>.ValidationFailure(errors);
        }

        Result<Period> existsResult = await _repository.GetByCompanyAndYearAsync(period.CompanyId , period.Year , cancellationToken);
        if (existsResult.IsSuccess)
        {
            return Result<Period>.Conflict($"{period.Year} yılı için dönem zaten mevcut.");
        }

        Result companyResult = await EnsureDatesAfterFoundationAsync(period , cancellationToken);
        if (companyResult.IsFailure)
        {
            return Result<Period>.Failure(companyResult.Message , companyResult.Status);
        }

        Result specialResult = await EnsureSpecialPeriodUniqueAsync(period , excludePeriodId: Guid.Empty , cancellationToken);
        if (specialResult.IsFailure)
        {
            return Result<Period>.Failure(specialResult.Message , specialResult.Status);
        }

        return await _repository.CreateAsync(period , cancellationToken);
    }

    public override async Task<Result> UpdateAsync(Period period , CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Error>? errors = await ValidateAsync(period , cancellationToken);
        if (errors is not null)
        {
            return Result.ValidationFailure(errors);
        }

        Result companyResult = await EnsureDatesAfterFoundationAsync(period , cancellationToken);
        if (companyResult.IsFailure)
        {
            return companyResult;
        }

        Result specialResult = await EnsureSpecialPeriodUniqueAsync(period , excludePeriodId: period.Id , cancellationToken);
        if (specialResult.IsFailure)
        {
            return specialResult;
        }

        return await base.UpdateAsync(period , cancellationToken);
    }
    #endregion Operations

    #region Helpers
    protected override void CopyFields(Period target , Period source)
    {
        // --- Ortak ---
        target.Year = source.Year;
        target.MonthNumber = source.MonthNumber;
        target.StartDate = source.StartDate;
        target.EndDate = source.EndDate;
        target.IsClosed = source.IsClosed;
        target.IsActive = source.IsActive;
        target.IsSpecial = source.IsSpecial;
        target.FinalizationDate = source.FinalizationDate;
        target.AccountingMethod = source.AccountingMethod;
        target.FirmClass = source.FirmClass;

        // --- Geçmiş Yıl Zararları ---
        target.OtherLoss = source.OtherLoss;
        target.ExceptionLoss = source.ExceptionLoss;

        // --- 1. Sınıf ---
        target.GrossWage = source.GrossWage;
        target.NetWage = source.NetWage;
        target.VatRate = source.VatRate;
        target.VatAmount = source.VatAmount;
        target.StorageRate = source.StorageRate;
        target.StorageAmount = source.StorageAmount;
        target.JournalStartNumber = source.JournalStartNumber;
        target.DeclarationType = source.DeclarationType;
        target.VoucherSortMode = source.VoucherSortMode;
        target.RenumberVouchers = source.RenumberVouchers;
        target.VoucherNumberLength = source.VoucherNumberLength;
        target.UseForeignCurrency = source.UseForeignCurrency;
        target.SystemCurrency = source.SystemCurrency;
        target.ExchangeRateMode = source.ExchangeRateMode;

        // --- 2. Sınıf ---
        target.LedgerType = source.LedgerType;
        target.IncomeStartNumber = source.IncomeStartNumber;
        target.ExpenseStartNumber = source.ExpenseStartNumber;
        target.CarryoverVat = source.CarryoverVat;
        target.IsVatTaxpayer = source.IsVatTaxpayer;
        target.UseDbsStockLedger = source.UseDbsStockLedger;
    }

    private async Task<Result> EnsureDatesAfterFoundationAsync(Period period , CancellationToken cancellationToken)
    {
        Result<Company> companyResult = await companyRepository.GetByIdAsync(period.CompanyId , cancellationToken);

        if (companyResult.IsFailure)
        {
            return Result.NotFound("Firma bulunamadı.");
        }

        DateTime foundationDate = companyResult.Data.FoundationDate;

        if (foundationDate == DateTime.MinValue)
        {
            return Result.Success();
        }

        if (period.StartDate < foundationDate)
        {
            return Result.BusinessRuleViolation($"Dönem başlangıç tarihi ({period.StartDate:dd.MM.yyyy}) firma kuruluş tarihinden ({foundationDate:dd.MM.yyyy}) önce olamaz.");
        }

        if (period.EndDate < foundationDate)
        {
            return Result.BusinessRuleViolation($"Dönem bitiş tarihi ({period.EndDate:dd.MM.yyyy}) firma kuruluş tarihinden ({foundationDate:dd.MM.yyyy}) önce olamaz.");
        }

        if (period.FinalizationDate != DateTime.MinValue && period.FinalizationDate < foundationDate)
        {
            return Result.BusinessRuleViolation($"Kapanış tarihi ({period.FinalizationDate:dd.MM.yyyy}) firma kuruluş tarihinden ({foundationDate:dd.MM.yyyy}) önce olamaz.");
        }

        return Result.Success();
    }

    private async Task<Result> EnsureSpecialPeriodUniqueAsync(Period period , Guid excludePeriodId , CancellationToken cancellationToken)
    {
        if (!period.IsSpecial)
        {
            return Result.Success();
        }

        Result<IReadOnlyList<Period>> periodsResult = await _repository.GetByCompanyIdAsync(period.CompanyId , cancellationToken);

        if (periodsResult.IsFailure)
        {
            return Result.Failure(periodsResult.Message , periodsResult.Status);
        }

        Period? existingSpecial = periodsResult.Data
            .Where(p => p.Id != excludePeriodId)
            .Where(p => p.Year == period.Year)
            .FirstOrDefault(p => p.IsSpecial);

        if (existingSpecial is not null)
        {
            return Result.BusinessRuleViolation($"{period.Year} yılı için zaten bir özel dönem mevcut. Özel dönem başlangıç tarihi: {existingSpecial.StartDate:dd.MM.yyyy}.");
        }

        return Result.Success();
    }
    #endregion Helpers
}