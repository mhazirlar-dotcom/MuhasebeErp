using Accounting.Core.Domain.Common;
using Accounting.Core.Domain.Constants;

namespace Accounting.Core.Domain.Entities;

public sealed class Company : AuditableEntity
{
    #region Properties
    // --- Kimlik ---
    public string ShortName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;

    // --- Hukuki ---
    public string LegalStatus { get; set; } = LookupCodes.LegalStatusOzel;
    public string LegalNature { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // --- Vergi ---
    public string TaxOffice { get; set; } = string.Empty;
    public DateTime FoundationDate { get; set; } = DateTime.MinValue;
    public string ActivityCode { get; set; } = string.Empty;

    // --- Sosyal Güvenlik / Meslek ---
    public string SocialSecurityInstitution { get; set; } = LookupCodes.SocialSecurityInstitutionSgk;
    public string ProfessionalOrganization { get; set; } = string.Empty;
    public string ProfessionalOrganizationMemberNumber { get; set; } = string.Empty;

    // --- Ticaret Sicil ---
    public string TradeRegistryOffice { get; set; } = string.Empty;
    public string TradeRegistryNumber { get; set; } = string.Empty;
    public string RegistryNumber { get; set; } = string.Empty;

    // --- Muhtasar Beyanname ---
    public string WithholdingDeclarationMethod { get; set; } = LookupCodes.WithholdingDeclarationMethodSirketBazinda;
    public string Create302RecordForWithholding { get; set; } = LookupCodes.Create302RecordOlustur;
    public DateTime PayrollCutoffDate { get; set; } = DateTime.MinValue;

    // --- Entegrasyon ---
    public string MerisNumber { get; set; } = string.Empty;
    public string TaxAuthorityUsername { get; set; } = string.Empty;
    public bool IsSpecialTaxpayer { get; set; } = false;

    // --- Bayraklar ---
    public bool SendReceiptDescriptionForDbs { get; set; } = false;
    public bool SendReceiptDescriptionForLedger { get; set; } = false;
    public bool AdminOnlyAccess { get; set; } = false;

    // --- Sistem ---
    public string ServerName { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string DbUserName { get; set; } = string.Empty;
    public string DbPasswordEncrypted { get; set; } = string.Empty;
    public bool IntegratedSecurity { get; set; } = true;
    public bool IsActive { get; set; } = true;
    #endregion Properties

    #region Relations
    public ICollection<UserCompany> UserCompanies { get; set; } = [];
    public ICollection<Period> Periods { get; set; } = [];
    #endregion Relations
}