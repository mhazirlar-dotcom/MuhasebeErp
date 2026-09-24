using Accounting.Core.Business.Extensions;
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

public partial class CompanyEditViewModel(ICompanyService companyService , IPeriodService periodService , ILookupValueService lookupValueService , IOptions<CompanyDatabaseOptions> options , IMessageService messageService) : ObservableObject, ITransientService
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
    public event EventHandler SaveCompleted = delegate { };
    public event EventHandler Cancelled = delegate { };
    #endregion Events

    #region Fields
    private Guid _companyId = Guid.Empty;
    private bool _isEditMode = false;
    private string _selectedSection = SectionFirma;

    private bool _periodsLoaded = false;
    private readonly HashSet<Guid> _deletedPeriodIds = [];
    private readonly HashSet<Guid> _existingPeriodIds = [];

    private DateTime _companyFoundationDate = DateTime.MinValue;
    #endregion Fields

    #region Properties
    [ObservableProperty]
    private string _title = "Firma Girişi";

    public bool IsEditMode
    {
        get => _isEditMode;
        set
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

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isBusyDonem;

    private Period _selectedPeriod = new();
    public Period SelectedPeriod
    {
        get => _selectedPeriod;
        set
        {
            if (SetProperty(ref _selectedPeriod , value ?? new Period()))
            {
                OnPropertyChanged(nameof(SelectedMonthName));
                OnPropertyChanged(nameof(SelectedFirmClass));
            }
        }
    }

    public string SelectedMonthName
    {
        get => SelectedPeriod.MonthNumber.ToString(TurkishCulture);
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (int.TryParse(value , NumberStyles.Integer , TurkishCulture , out int monthNumber))
            {
                SelectedPeriod.MonthNumber = monthNumber;
                OnPropertyChanged();
            }
        }
    }

    public int SelectedFirmClass
    {
        get => SelectedPeriod.FirmClass;
        set
        {
            if (SelectedPeriod.FirmClass == value)
            {
                return;
            }

            SelectedPeriod.FirmClass = value;
            OnPropertyChanged();
        }
    }

    public bool IsNotBusy => !IsBusy;

    public bool IsNotBusyDonem => !IsBusyDonem;

    public bool IsNotEditMode => !IsEditMode;

    public bool IsFirmaSection => SelectedSection == SectionFirma;

    public bool IsDonemSection => SelectedSection == SectionDonem;

    public bool IsPlaceholderSection => !IsFirmaSection && !IsDonemSection;

    public string PlaceholderMessage => IsEditMode
        ? "Bu bölüm henüz geliştirme aşamasındadır."
        : "Bu bölümü kullanmak için önce firmayı kaydedin.";

    public ObservableCollection<LookupValue> LegalStatusList { get; } = [];
    public ObservableCollection<LookupValue> LegalNatureList { get; } = [];
    public ObservableCollection<LookupValue> SocialSecurityInstitutionList { get; } = [];
    public ObservableCollection<LookupValue> ProfessionalOrganizationList { get; } = [];
    public ObservableCollection<LookupValue> TradeRegistryOfficeList { get; } = [];
    public ObservableCollection<LookupValue> WithholdingDeclarationMethodList { get; } = [];
    public ObservableCollection<LookupValue> Create302RecordList { get; } = [];
    public ObservableCollection<LookupValue> TaxOfficeList { get; } = [];
    public ObservableCollection<LookupValue> ActivityCodeList { get; } = [];

    public ObservableCollection<LookupValue> AccountingMethodList { get; } = [];
    public ObservableCollection<LookupValue> CurrencyList { get; } = [];
    public ObservableCollection<LookupValue> DeclarationTypeList { get; } = [];
    public ObservableCollection<LookupValue> ExchangeRateModeList { get; } = [];
    public ObservableCollection<LookupValue> LedgerTypeList { get; } = [];
    public ObservableCollection<LookupValue> VatRateList { get; } = [];
    public ObservableCollection<LookupValue> VoucherNumberLengthList { get; } = [];
    public ObservableCollection<LookupValue> VoucherSortModeList { get; } = [];
    public ObservableCollection<LookupValue> MonthNameList { get; } = [];

    public ObservableCollection<FirmClassItem> FirmClassList { get; } =
    [
        new FirmClassItem(FirmClasses.Class1 , "1. Sınıf"),
        new FirmClassItem(FirmClasses.Class2 , "2. Sınıf")
    ];

    public ObservableCollection<Period> Periods { get; } = [];
    #endregion Properties

    #region Operations
    public async Task LoadAsync(Guid companyId)
    {
        ResetForm();

        await LoadLookupsAsync();

        CompanyDatabaseOptions dbOptions = options.Value;

        if (companyId == Guid.Empty)
        {
            ServerName = dbOptions.ServerName;
            DatabaseName = "Firma kaydedildiğinde otomatik üretilecek";
            return;
        }

        Title = "Firma Düzenle";
        _companyId = companyId;
        IsEditMode = true;

        Result<Company> result = await companyService.GetByIdAsync(companyId);
        if (result.IsFailure)
        {
            messageService.ShowErrors("Firma yüklenemedi" , result.Errors);
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

        _companyFoundationDate = company.FoundationDate;
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
            SaveCompleted.Invoke(this , EventArgs.Empty);
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
            await SavePeriodsAsync();
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

        if (section == SectionDonem && IsEditMode && !_periodsLoaded)
        {
            await LoadPeriodsAsync();
        }
    }

    [RelayCommand]
    private void AddPeriod()
    {
        if (!IsEditMode)
        {
            return;
        }

        if (MonthNameList.Count == 0)
        {
            messageService.ShowWarning("Ay listesi yüklenemedi. Lütfen API'nin çalıştığından ve seed verisinin yüklendiğinden emin olun.");
            return;
        }

        DateTime foundationDate = _companyFoundationDate == DateTime.MinValue ? FoundationDate : _companyFoundationDate;

        AddPeriodDialogViewModel dialogViewModel = new(foundationDate , [.. Periods] , [.. MonthNameList]);
        Windows.AddPeriodDialog dialog = new(dialogViewModel);

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        Period? newPeriod = dialogViewModel.Result;

        if (newPeriod is null)
        {
            return;
        }

        newPeriod.CompanyId = _companyId;

        Periods.Add(newPeriod);
        SelectedPeriod = newPeriod;
    }

    [RelayCommand]
    private void DeletePeriod()
    {
        if (!IsEditMode)
        {
            return;
        }

        if (SelectedPeriod is null || SelectedPeriod.Id == Guid.Empty)
        {
            messageService.ShowWarning("Silinecek dönemi seçin.");
            return;
        }

        if (!Periods.Contains(SelectedPeriod))
        {
            messageService.ShowWarning("Silinecek dönemi seçin.");
            return;
        }

        if (_existingPeriodIds.Contains(SelectedPeriod.Id))
        {
            _deletedPeriodIds.Add(SelectedPeriod.Id);
        }

        Periods.Remove(SelectedPeriod);
        SelectedPeriod = new Period();
    }

    [RelayCommand]
    private async Task SavePeriodsAsync()
    {
        if (_companyId == Guid.Empty)
        {
            messageService.ShowWarning("Önce firmayı kaydedin.");
            return;
        }

        IsBusyDonem = true;

        try
        {
            foreach (Guid id in _deletedPeriodIds)
            {
                Result deleteResult = await periodService.DeleteAsync(id);
                if (deleteResult.IsFailure)
                {
                    messageService.ShowErrors("Dönem silinemedi" , deleteResult.Errors);
                    return;
                }
            }

            foreach (Period period in Periods)
            {
                period.CompanyId = _companyId;

                if (!_existingPeriodIds.Contains(period.Id))
                {
                    Result<Period> createResult = await periodService.CreateAsync(period);
                    if (createResult.IsFailure)
                    {
                        messageService.ShowErrors("Dönem kaydedilemedi" , createResult.Errors);
                        return;
                    }
                }
                else
                {
                    Result updateResult = await periodService.UpdateAsync(period);
                    if (updateResult.IsFailure)
                    {
                        messageService.ShowErrors("Dönem güncellenemedi" , updateResult.Errors);
                        return;
                    }
                }
            }

            messageService.ShowSuccess("Dönemler kaydedildi.");
        }
        finally
        {
            IsBusyDonem = false;
        }

        _periodsLoaded = false;
        await LoadPeriodsAsync();
    }

    [RelayCommand]
    private async Task CancelPeriodsAsync()
    {
        _periodsLoaded = false;
        await LoadPeriodsAsync();
        messageService.ShowInfo("Değişiklikler geri alındı.");
    }
    #endregion Operations

    #region Helpers
    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotBusy));
    }

    partial void OnIsBusyDonemChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotBusyDonem));
    }

    private void ResetForm()
    {
        _companyId = Guid.Empty;
        _companyFoundationDate = DateTime.MinValue;
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

        ResetPeriodState();
    }

    private void ResetPeriodState()
    {
        _periodsLoaded = false;
        _deletedPeriodIds.Clear();
        _existingPeriodIds.Clear();
        Periods.Clear();
        SelectedPeriod = new Period();
        IsBusyDonem = false;
    }

    private async Task LoadPeriodsAsync()
    {
        ResetPeriodState();

        if (_companyId == Guid.Empty)
        {
            _periodsLoaded = true;
            return;
        }

        Result<IReadOnlyList<Period>> result = await periodService.GetByCompanyIdAsync(_companyId);

        if (result.IsFailure)
        {
            messageService.ShowErrors("Dönem listesi yüklenemedi" , result.Errors);
            return;
        }

        foreach (Period period in result.Data)
        {
            Periods.Add(period);
            _existingPeriodIds.Add(period.Id);
        }

        _periodsLoaded = true;
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
            (LookupTypes.ActivityCode , ActivityCodeList),
            (LookupTypes.AccountingMethod , AccountingMethodList),
            (LookupTypes.Currency , CurrencyList),
            (LookupTypes.DeclarationType , DeclarationTypeList),
            (LookupTypes.ExchangeRateMode , ExchangeRateModeList),
            (LookupTypes.LedgerType , LedgerTypeList),
            (LookupTypes.VatRate , VatRateList),
            (LookupTypes.VoucherNumberLength , VoucherNumberLengthList),
            (LookupTypes.VoucherSortMode , VoucherSortModeList),
            (LookupTypes.MonthName , MonthNameList)
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

        Result<IReadOnlyList<LookupValue>> result = await lookupValueService.GetByTypeAsync(type);

        if (result.IsFailure)
        {
            return;
        }

        IEnumerable<LookupValue> sorted = type == LookupTypes.MonthName
            ? result.Data.OrderBy(x => int.TryParse(x.Code , out int n) ? n : int.MaxValue)
            : result.Data.OrderBy(x => x.Name , StringComparer.Create(TurkishCulture , true));

        foreach (LookupValue item in sorted)
        {
            target.Add(item);
        }
    }

    private Company BuildCompanyFromForm()
    {
        Company company = new()
        {
            ShortName = ShortName,
            Name = Name,
            IdentityNumber = IdentityNumber,
            TaxNumber = TaxNumber,
            LegalStatus = LegalStatus,
            LegalNature = LegalNature,
            Description = Description,
            TaxOffice = TaxOffice,
            FoundationDate = FoundationDate,
            ActivityCode = ActivityCode,
            SocialSecurityInstitution = SocialSecurityInstitution,
            ProfessionalOrganization = ProfessionalOrganization,
            ProfessionalOrganizationMemberNumber = ProfessionalOrganizationMemberNumber,
            TradeRegistryOffice = TradeRegistryOffice,
            TradeRegistryNumber = TradeRegistryNumber,
            RegistryNumber = RegistryNumber,
            WithholdingDeclarationMethod = WithholdingDeclarationMethod,
            Create302RecordForWithholding = Create302RecordForWithholding,
            PayrollCutoffDate = PayrollCutoffDate,
            MerisNumber = MerisNumber,
            TaxAuthorityUsername = TaxAuthorityUsername,
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

        Result<Company> saveResult = await companyService.CreateAsync(company);
        if (saveResult.IsFailure)
        {
            messageService.ShowErrors("Firma kaydedilemedi" , saveResult.Errors);
            return false;
        }

        _companyId = company.Id;
        _companyFoundationDate = company.FoundationDate;
        DatabaseName = company.DatabaseName;
        Title = "Firma Düzenle";
        IsEditMode = true;

        messageService.ShowSuccess("Firma kaydedildi.");
        return true;
    }

    private async Task<bool> UpdateAsync()
    {
        Company company = BuildCompanyFromForm();

        Result saveResult = await companyService.UpdateAsync(company);
        if (saveResult.IsFailure)
        {
            messageService.ShowErrors("Firma güncellenemedi" , saveResult.Errors);
            return false;
        }

        _companyFoundationDate = company.FoundationDate;

        messageService.ShowSuccess("Firma güncellendi.");
        return true;
    }
    #endregion Helpers
}