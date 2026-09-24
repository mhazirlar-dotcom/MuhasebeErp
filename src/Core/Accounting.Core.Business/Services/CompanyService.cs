using Accounting.Core.Business.Interfaces;
using Accounting.Core.Business.Interfaces.Repositories;
using Accounting.Core.Business.Interfaces.Services;
using Accounting.Core.Business.Options;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Results;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Accounting.Core.Business.Services;

public sealed class CompanyService(ICompanyRepository companyRepository , IDatabaseInitializer databaseInitializer , IOptions<CompanyDatabaseOptions> options , ICurrentUser currentUser , IValidator<Company> validator) : ServiceBase<Company , ICompanyRepository>(companyRepository , validator), ICompanyService
{
    #region Operations
    public async Task<Result<IReadOnlyList<Company>>> GetByUserIdAsync(bool includeActive = true , bool includePassive = false , CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated)
        {
            return Result<IReadOnlyList<Company>>.Unauthorized("Kullanıcı oturumu bulunamadı.");
        }

        return await _repository.GetByUserIdAsync(currentUser.UserId , includeActive , includePassive , cancellationToken);
    }

    public override async Task<Result<Company>> CreateAsync(Company company , CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Error>? errors = await ValidateAsync(company , cancellationToken);
        if (errors is not null)
        {
            return Result<Company>.ValidationFailure(errors);
        }

        if (!currentUser.IsAuthenticated)
        {
            return Result<Company>.Unauthorized("Kullanıcı oturumu bulunamadı.");
        }

        ApplySystemFields(company);

        Result<Company> addResult = await _repository.AddWithUserAsync(company , currentUser.UserId , isDefault: false , cancellationToken);
        if (addResult.IsFailure)
        {
            return addResult;
        }

        Result initResult = await databaseInitializer.InitializeCompanyDatabaseAsync(company , cancellationToken);
        if (initResult.IsFailure)
        {
            return Result<Company>.Failure($"Firma kaydedildi ama veritabanı kurulamadı: {initResult.Message}" , initResult.Status);
        }

        return addResult;
    }

    public override Task<Result> DeleteAsync(Guid id , CancellationToken cancellationToken = default)
    {
        // GLOBAL KURAL: Firma silme = soft delete (IsActive = false)
        return SetActiveAsync(id , isActive: false , cancellationToken);
    }

    public async Task<Result> SetActiveAsync(Guid id , bool isActive , CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure("Firma Id zorunludur." , ResultStatus.ValidationError);
        }

        Result<Company> getResult = await _repository.GetByIdForUpdateAsync(id , cancellationToken);
        if (getResult.IsFailure)
        {
            return Result.NotFound(getResult.Message);
        }

        Company company = getResult.Data;
        company.IsActive = isActive;

        return await _repository.SaveChangesAsync(cancellationToken);
    }
    #endregion Operations

    #region Helpers
    protected override void CopyFields(Company target , Company source)
    {
        target.ShortName = source.ShortName;
        target.Name = source.Name;
        target.IdentityNumber = source.IdentityNumber;
        target.TaxNumber = source.TaxNumber;
        target.LegalStatus = source.LegalStatus;
        target.LegalNature = source.LegalNature;
        target.Description = source.Description;
        target.TaxOffice = source.TaxOffice;
        target.FoundationDate = source.FoundationDate;
        target.ActivityCode = source.ActivityCode;
        target.SocialSecurityInstitution = source.SocialSecurityInstitution;
        target.ProfessionalOrganization = source.ProfessionalOrganization;
        target.ProfessionalOrganizationMemberNumber = source.ProfessionalOrganizationMemberNumber;
        target.TradeRegistryOffice = source.TradeRegistryOffice;
        target.TradeRegistryNumber = source.TradeRegistryNumber;
        target.RegistryNumber = source.RegistryNumber;
        target.WithholdingDeclarationMethod = source.WithholdingDeclarationMethod;
        target.Create302RecordForWithholding = source.Create302RecordForWithholding;
        target.PayrollCutoffDate = source.PayrollCutoffDate;
        target.MerisNumber = source.MerisNumber;
        target.TaxAuthorityUsername = source.TaxAuthorityUsername;
        target.IsSpecialTaxpayer = source.IsSpecialTaxpayer;
        target.SendReceiptDescriptionForDbs = source.SendReceiptDescriptionForDbs;
        target.SendReceiptDescriptionForLedger = source.SendReceiptDescriptionForLedger;
        target.AdminOnlyAccess = source.AdminOnlyAccess;
        target.IsActive = source.IsActive;
    }

    private void ApplySystemFields(Company company)
    {
        CompanyDatabaseOptions dbOptions = options.Value;

        company.ServerName = dbOptions.ServerName;
        company.DatabaseName = dbOptions.BuildDatabaseName(company.Id);
        company.IntegratedSecurity = dbOptions.IntegratedSecurity;
        company.DbUserName = dbOptions.IntegratedSecurity ? string.Empty : dbOptions.DbUserName;
        company.DbPasswordEncrypted = string.Empty;
    }
    #endregion Helpers
}