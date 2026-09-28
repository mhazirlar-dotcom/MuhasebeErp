using Accounting.Core.Business.Interfaces.CrossCutting;
using Accounting.Core.Business.Interfaces.Services;
using Accounting.Core.Business.Options;
using Accounting.Core.Domain.Constants;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Markers;
using Accounting.Shared.Results;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Accounting.Desktop.ViewModels;

public partial class CompanyEditViewModel : ObservableObject, ITransientService
{
    #region Constants
    public const string SectionFirma = "Firma";
    public const string SectionDonem = "Dönem";
    public const string SectionOrtaklar = "Ortaklar";
    public const string SectionAdres = "Adres";
    public const string SectionIletisim = "İletişim";
    public const string SectionKontak = "Kontak";

    private static readonly CultureInfo TurkishCulture = new("tr-TR");
    #endregion Constants

    #region Events
    public event EventHandler<Guid> SaveCompleted = delegate { };
    public event EventHandler Cancelled = delegate { };
    #endregion Events

    #region Fields
    private readonly ICompanyService _companyService;
    private readonly ILookupValueService _lookupValueService;
    private readonly IMessageService _messageService;
    private readonly CompanyDatabaseOptions _dbOptions;

    private Guid _companyId = Guid.Empty;
    private bool _isEditMode = false;
    private string _selectedSection = SectionFirma;
    #endregion Fields

    #region Constructor
    public CompanyEditViewModel(
        ICompanyService companyService ,
        ILookupValueService lookupValueService ,
        IMessageService messageService ,
        IOptions<CompanyDatabaseOptions> options ,
        PeriodListViewModel periodList)
    {
        _companyService = companyService;
        _lookupValueService = lookupValueService;
        _messageService = messageService;
        _dbOptions = options.Value;

        PeriodList = periodList;
    }
    #endregion Constructor

    #region Properties
    [ObservableProperty]
    private string _title = "Firma Girişi";

    [ObservableProperty]
    private bool _isBusy;

    public bool IsNotBusy => !IsBusy;

    public bool IsEditMode
    {
        get => _isEditMode;
        private set
        {
            if (SetProperty(ref _isEditMode , value))
            {
                OnPropertyChanged(nameof(IsNotEditMode));
                OnPropertyChanged(nameof(PlaceholderMessage));
            }
        }
    }

    public string SelectedSection
    {
        get => _selectedSection;
        set
        {
            if (SetProperty(ref _selectedSection , value))
            {
                OnPropertyChanged(nameof(IsFirmaSection));
                OnPropertyChanged(nameof(IsDonemSection));
                OnPropertyChanged(nameof(IsPlaceholderSection));
                OnPropertyChanged(nameof(PlaceholderMessage));
            }
        }
    }

    [ObservableProperty]
    private string _shortName = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _identityNumber = string.Empty;

    [ObservableProperty]
    private string _taxNumber = string.Empty;

    [ObservableProperty]
    private string _legalStatus = LookupCodes.LegalStatusOzel;

    [ObservableProperty]
    private string _legalNature = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _taxOffice = string.Empty;

    private DateTime _foundationDate = DateTime.MinValue;
    public DateTime FoundationDate
    {
        get => _foundationDate;
        set
        {
            if (SetProperty(ref _foundationDate , value))
            {
                OnPropertyChanged(nameof(FoundationDateNullable));
            }
        }
    }

    public DateTime? FoundationDateNullable
    {
        get => _foundationDate == DateTime.MinValue ? null : _foundationDate;
        set => FoundationDate = value ?? DateTime.MinValue;
    }

    [ObservableProperty]
    private string _activityCode = string.Empty;

    [ObservableProperty]
    private string _socialSecurityInstitution = LookupCodes.SocialSecurityInstitutionSgk;

    [ObservableProperty]
    private string _professionalOrganization = string.Empty;

    [ObservableProperty]
    private string _professionalOrganizationMemberNumber = string.Empty;

    [ObservableProperty]
    private string _tradeRegistryOffice = string.Empty;

    [ObservableProperty]
    private string _tradeRegistryNumber = string.Empty;

    [ObservableProperty]
    private string _registryNumber = string.Empty;

    [ObservableProperty]
    private string _withholdingDeclarationMethod = LookupCodes.WithholdingDeclarationMethodSirketBazinda;

    [ObservableProperty]
    private string _create302RecordForWithholding = LookupCodes.Create302RecordOlustur;

    private DateTime _payrollCutoffDate = DateTime.MinValue;
    public DateTime PayrollCutoffDate
    {
        get => _payrollCutoffDate;
        set
        {
            if (SetProperty(ref _payrollCutoffDate , value))
            {
                OnPropertyChanged(nameof(PayrollCutoffDateNullable));
            }
        }
    }

    public DateTime? PayrollCutoffDateNullable
    {
        get => _payrollCutoffDate == DateTime.MinValue ? null : _payrollCutoffDate;
        set => PayrollCutoffDate = value ?? DateTime.MinValue;
    }

    [ObservableProperty]
    private string _merisNumber = string.Empty;

    [ObservableProperty]
    private string _taxAuthorityUsername = string.Empty;

    [ObservableProperty]
    private bool _isSpecialTaxpayer;

    [ObservableProperty]
    private bool _sendReceiptDescriptionForDbs;

    [ObservableProperty]
    private bool _sendReceiptDescriptionForLedger;

    [ObservableProperty]
    private bool _adminOnlyAccess;

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private string _serverName = string.Empty;

    [ObservableProperty]
    private string _databaseName = string.Empty;

    public bool IsNotEditMode => !IsEditMode;

    public bool IsFirmaSection => SelectedSection == SectionFirma;

    public bool IsDonemSection => SelectedSection == SectionDonem;

    public bool IsPlaceholderSection => !IsFirmaSection && !IsDonemSection;

    public string PlaceholderMessage => IsEditMode
        ? "Bu bölüm henüz geliştirme aşamasındadır."
        : "Bu bölümü kullanmak için önce firmayı kaydedin.";

    public PeriodListViewModel PeriodList { get; }

    public ObservableCollection<LookupValue> LegalStatusList { get; } = [];
    public ObservableCollection<LookupValue> LegalNatureList { get; } = [];
    public ObservableCollection<LookupValue> SocialSecurityInstitutionList { get; } = [];
    public ObservableCollection<LookupValue> ProfessionalOrganizationList { get; } = [];
    public ObservableCollection<LookupValue> TradeRegistryOfficeList { get; } = [];
    public ObservableCollection<LookupValue> WithholdingDeclarationMethodList { get; } = [];
    public ObservableCollection<LookupValue> Create302RecordList { get; } = [];
    public ObservableCollection<LookupValue> TaxOfficeList { get; } = [];
    public ObservableCollection<LookupValue> ActivityCodeList { get; } = [];
    #endregion Properties

    #region Operations
    public async Task LoadAsync(Guid companyId , string? initialSection = null)
    {
        ResetForm();

        await LoadLookupsAsync();

        if (companyId == Guid.Empty)
        {
            ServerName = _dbOptions.ServerName;
            DatabaseName = "Firma kaydedildiğinde otomatik üretilecek";
            return;
        }

        Title = "Firma Düzenle";
        _companyId = companyId;
        IsEditMode = true;

        Result<Company> result = await _companyService.GetByIdAsync(companyId);
        if (result.IsFailure)
        {
            _messageService.ShowErrors("Firma yüklenemedi" , result.Errors);
            return;
        }

        Company company = result.Data;

        ShortName = company.ShortName;
        Name = company.Name;
        IdentityNumber = company.IdentityNumber;
        TaxNumber = company.TaxNumber;
        LegalStatus = company.LegalStatus;
        LegalNature = company.LegalNature;
        Description = company.Description;
        TaxOffice = company.TaxOffice;
        FoundationDate = company.FoundationDate;
        ActivityCode = company.ActivityCode;
        SocialSecurityInstitution = company.SocialSecurityInstitution;
        ProfessionalOrganization = company.ProfessionalOrganization;
        ProfessionalOrganizationMemberNumber = company.ProfessionalOrganizationMemberNumber;
        TradeRegistryOffice = company.TradeRegistryOffice;
        TradeRegistryNumber = company.TradeRegistryNumber;
        RegistryNumber = company.RegistryNumber;
        WithholdingDeclarationMethod = company.WithholdingDeclarationMethod;
        Create302RecordForWithholding = company.Create302RecordForWithholding;
        PayrollCutoffDate = company.PayrollCutoffDate;
        MerisNumber = company.MerisNumber;
        TaxAuthorityUsername = company.TaxAuthorityUsername;
        IsSpecialTaxpayer = company.IsSpecialTaxpayer;
        SendReceiptDescriptionForDbs = company.SendReceiptDescriptionForDbs;
        SendReceiptDescriptionForLedger = company.SendReceiptDescriptionForLedger;
        AdminOnlyAccess = company.AdminOnlyAccess;
        IsActive = company.IsActive;
        ServerName = company.ServerName;
        DatabaseName = company.DatabaseName;

        if (!string.IsNullOrWhiteSpace(initialSection))
        {
            await SelectSectionAsync(initialSection);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        IsBusy = true;
        bool succeeded = false;

        try
        {
            if (_companyId == Guid.Empty)
            {
                succeeded = await CreateAsync();
            }
            else
            {
                succeeded = await UpdateAsync();
            }
        }
        finally
        {
            IsBusy = false;
        }

        if (succeeded)
        {
            SaveCompleted.Invoke(this , _companyId);
        }
    }

    [RelayCommand]
    private async Task SaveActiveSectionAsync()
    {
        if (IsFirmaSection)
        {
            await SaveAsync();
            return;
        }

        if (IsDonemSection)
        {
            // Dönem sekmesi artık ayrı bir view (PeriodEditView) tarafından yönetilir.
            return;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        Cancelled.Invoke(this , EventArgs.Empty);
    }

    [RelayCommand]
    private async Task SelectSectionAsync(string section)
    {
        if (string.IsNullOrWhiteSpace(section))
        {
            return;
        }

        if (section != SectionFirma && !IsEditMode)
        {
            return;
        }

        SelectedSection = section;

        if (section == SectionDonem && IsEditMode)
        {
            await PeriodList.LoadAsync(_companyId);
        }
    }
    #endregion Operations

    #region Helpers
    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotBusy));
    }

    private void ResetForm()
    {
        _companyId = Guid.Empty;
        Title = "Firma Girişi";
        IsEditMode = false;
        SelectedSection = SectionFirma;

        ShortName = string.Empty;
        Name = string.Empty;
        IdentityNumber = string.Empty;
        TaxNumber = string.Empty;
        LegalStatus = LookupCodes.LegalStatusOzel;
        LegalNature = string.Empty;
        Description = string.Empty;
        TaxOffice = string.Empty;
        FoundationDate = DateTime.MinValue;
        ActivityCode = string.Empty;
        SocialSecurityInstitution = LookupCodes.SocialSecurityInstitutionSgk;
        ProfessionalOrganization = string.Empty;
        ProfessionalOrganizationMemberNumber = string.Empty;
        TradeRegistryOffice = string.Empty;
        TradeRegistryNumber = string.Empty;
        RegistryNumber = string.Empty;
        WithholdingDeclarationMethod = LookupCodes.WithholdingDeclarationMethodSirketBazinda;
        Create302RecordForWithholding = LookupCodes.Create302RecordOlustur;
        PayrollCutoffDate = DateTime.MinValue;
        MerisNumber = string.Empty;
        TaxAuthorityUsername = string.Empty;
        IsSpecialTaxpayer = false;
        SendReceiptDescriptionForDbs = false;
        SendReceiptDescriptionForLedger = false;
        AdminOnlyAccess = false;
        IsActive = true;
        ServerName = string.Empty;
        DatabaseName = string.Empty;
    }

    private async Task LoadLookupsAsync()
    {
        (string Type, ObservableCollection<LookupValue> Target)[] lookups =
        [
            (LookupTypes.LegalStatus , LegalStatusList),
            (LookupTypes.LegalNature , LegalNatureList),
            (LookupTypes.SocialSecurityInstitution , SocialSecurityInstitutionList),
            (LookupTypes.ProfessionalOrganization , ProfessionalOrganizationList),
            (LookupTypes.TradeRegistryOffice , TradeRegistryOfficeList),
            (LookupTypes.WithholdingDeclarationMethod , WithholdingDeclarationMethodList),
            (LookupTypes.Create302Record , Create302RecordList),
            (LookupTypes.TaxOffice , TaxOfficeList),
            (LookupTypes.ActivityCode , ActivityCodeList)
        ];

        foreach ((string type , ObservableCollection<LookupValue> target) in lookups)
        {
            await LoadLookupAsync(type , target);
        }
    }

    private async Task LoadLookupAsync(string type , ObservableCollection<LookupValue> target)
    {
        if (target.Count > 0)
        {
            return;
        }

        Result<IReadOnlyList<LookupValue>> result = await _lookupValueService.GetByTypeAsync(type);

        if (result.IsFailure)
        {
            return;
        }

        IEnumerable<LookupValue> sorted = result.Data.OrderBy(x => x.Name , StringComparer.Create(TurkishCulture , true));

        foreach (LookupValue item in sorted)
        {
            target.Add(item);
        }
    }

    private Company BuildCompanyFromForm()
    {
        Company company = new()
        {
            ShortName = ShortName ?? string.Empty,
            Name = Name ?? string.Empty,
            IdentityNumber = IdentityNumber ?? string.Empty,
            TaxNumber = TaxNumber ?? string.Empty,
            LegalStatus = LegalStatus ?? string.Empty,
            LegalNature = LegalNature ?? string.Empty,
            Description = Description ?? string.Empty,
            TaxOffice = TaxOffice ?? string.Empty,
            FoundationDate = FoundationDate,
            ActivityCode = ActivityCode ?? string.Empty,
            SocialSecurityInstitution = SocialSecurityInstitution ?? string.Empty,
            ProfessionalOrganization = ProfessionalOrganization ?? string.Empty,
            ProfessionalOrganizationMemberNumber = ProfessionalOrganizationMemberNumber ?? string.Empty,
            TradeRegistryOffice = TradeRegistryOffice ?? string.Empty,
            TradeRegistryNumber = TradeRegistryNumber ?? string.Empty,
            RegistryNumber = RegistryNumber ?? string.Empty,
            WithholdingDeclarationMethod = WithholdingDeclarationMethod ?? string.Empty,
            Create302RecordForWithholding = Create302RecordForWithholding ?? string.Empty,
            PayrollCutoffDate = PayrollCutoffDate,
            MerisNumber = MerisNumber ?? string.Empty,
            TaxAuthorityUsername = TaxAuthorityUsername ?? string.Empty,
            IsSpecialTaxpayer = IsSpecialTaxpayer,
            SendReceiptDescriptionForDbs = SendReceiptDescriptionForDbs,
            SendReceiptDescriptionForLedger = SendReceiptDescriptionForLedger,
            AdminOnlyAccess = AdminOnlyAccess,
            IsActive = IsActive
        };

        if (_companyId != Guid.Empty)
        {
            company.Id = _companyId;
        }

        return company;
    }

    private async Task<bool> CreateAsync()
    {
        Company company = BuildCompanyFromForm();

        Result<Company> saveResult = await _companyService.CreateAsync(company);
        if (saveResult.IsFailure)
        {
            _messageService.ShowErrors("Firma kaydedilemedi" , saveResult.Errors);
            return false;
        }

        _companyId = company.Id;
        DatabaseName = company.DatabaseName;
        Title = "Firma Düzenle";
        IsEditMode = true;

        _messageService.ShowSuccess("Firma kaydedildi.");
        return true;
    }

    private async Task<bool> UpdateAsync()
    {
        Company company = BuildCompanyFromForm();

        Result saveResult = await _companyService.UpdateAsync(company);
        if (saveResult.IsFailure)
        {
            _messageService.ShowErrors("Firma güncellenemedi" , saveResult.Errors);
            return false;
        }

        _messageService.ShowSuccess("Firma güncellendi.");
        return true;
    }
    #endregion Helpers
}