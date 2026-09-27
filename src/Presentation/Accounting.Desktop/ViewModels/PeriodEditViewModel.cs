using Accounting.Core.Business.Interfaces;
using Accounting.Core.Business.Interfaces.CrossCutting;
using Accounting.Core.Business.Interfaces.Services;
using Accounting.Core.Domain.Constants;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Markers;
using Accounting.Shared.Results;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Accounting.Desktop.ViewModels;

public partial class PeriodEditViewModel(
    IPeriodService periodService ,
    ICompanyService companyService ,
    ILookupValueService lookupValueService ,
    IMessageService messageService ,
    IClock clock) : ObservableObject, ITransientService
{
    #region Constants
    private const int FallbackFoundationYear = 1900;
    private const int PeriodMonthCount = 12;
    private const int MonthJanuary = 1;

    private static readonly CultureInfo TurkishCulture = new("tr-TR");
    #endregion Constants

    #region Events
    public event EventHandler SaveCompleted = delegate { };
    public event EventHandler Cancelled = delegate { };
    #endregion Events

    #region Fields
    private readonly IPeriodService _periodService = periodService;
    private readonly ICompanyService _companyService = companyService;
    private readonly ILookupValueService _lookupValueService = lookupValueService;
    private readonly IMessageService _messageService = messageService;
    private readonly IClock _clock = clock;

    private Guid _companyId = Guid.Empty;
    private Guid _periodId = Guid.Empty;
    private bool _isEditMode = false;
    private bool _isInitializing = true;
    private bool _isVatAmountOverridden = false;
    private bool _isStorageAmountOverridden = false;
    private bool _suppressSpecialConfirm = false;

    private DateTime _companyFoundationDate = DateTime.MinValue;
    private IReadOnlyList<Period> _existingPeriods = [];

    #endregion Fields
    #region Constructor
    #endregion Constructor

    #region Properties
    [ObservableProperty]
    private string _title = "Dönem Ekle";

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
                OnPropertyChanged(nameof(IsYearEditable));
            }
        }
    }

    public bool IsYearEditable => !IsEditMode;

    // --- Yıl / Ay / Özel Dönem ---
    public ObservableCollection<int> AvailableYears { get; } = [];

    private int _selectedYear;
    public int SelectedYear
    {
        get => _selectedYear;
        set
        {
            if (SetProperty(ref _selectedYear , value) && !_isInitializing)
            {
                RecalculateDates();
            }
        }
    }

    public ObservableCollection<LookupValue> AvailableMonths { get; } = [];

    private LookupValue _selectedMonth = null!;
    public LookupValue SelectedMonth
    {
        get => _selectedMonth;
        set
        {
            if (SetProperty(ref _selectedMonth , value ?? null!) && !_isInitializing)
            {
                RecalculateDates();
            }
        }
    }

    private bool _isSpecialPeriod;
    public bool IsSpecialPeriod
    {
        get => _isSpecialPeriod;
        set
        {
            if (_isSpecialPeriod == value)
            {
                return;
            }

            if (!_suppressSpecialConfirm && value && !_isInitializing)
            {
                bool confirmed = _messageService.Confirm(
                    "Özel Dönem Onayı" ,
                    "Özel dönem; standart dönemden farklı bir başlangıç ayı ile 12 aylık bir mali dönem oluşturur.\n\n" +
                    "Bu işlem, aynı yıl için farklı bir dönem yapısı oluşturacağından dikkatli kullanılmalıdır.\n\n" +
                    "Özel dönem oluşturmak istediğinize emin misiniz?");

                if (!confirmed)
                {
                    OnPropertyChanged(nameof(IsSpecialPeriod));
                    OnPropertyChanged(nameof(IsMonthSelectorVisible));
                    return;
                }
            }

            if (SetProperty(ref _isSpecialPeriod , value))
            {
                OnPropertyChanged(nameof(IsMonthSelectorVisible));

                if (!_suppressSpecialConfirm && !value && !_isInitializing && AvailableMonths.Count > 0)
                {
                    _isInitializing = true;
                    SelectedMonth = AvailableMonths.FirstOrDefault(m => ParseMonthNumber(m.Code) == MonthJanuary) ?? AvailableMonths[0];
                    _isInitializing = false;
                }

                if (!_isInitializing)
                {
                    RecalculateDates();
                }
            }
        }
    }

    public bool IsMonthSelectorVisible => IsSpecialPeriod;

    // --- Hesaplanan tarihler (readonly) ---
    [ObservableProperty]
    private DateTime _startDate = DateTime.MinValue;

    [ObservableProperty]
    private DateTime _endDate = DateTime.MinValue;

    public DateTime? StartDateNullable
    {
        get => StartDate == DateTime.MinValue ? null : StartDate;
        set => StartDate = value ?? DateTime.MinValue;
    }

    public DateTime? EndDateNullable
    {
        get => EndDate == DateTime.MinValue ? null : EndDate;
        set => EndDate = value ?? DateTime.MinValue;
    }

    // --- ORTAK BİLGİLER ---
    private DateTime _finalizationDate = DateTime.MinValue;
    public DateTime FinalizationDate
    {
        get => _finalizationDate;
        set
        {
            if (SetProperty(ref _finalizationDate , value))
            {
                OnPropertyChanged(nameof(FinalizationDateNullable));
            }
        }
    }

    public DateTime? FinalizationDateNullable
    {
        get => _finalizationDate == DateTime.MinValue ? null : _finalizationDate;
        set => FinalizationDate = value ?? DateTime.MinValue;
    }

    [ObservableProperty]
    private string _accountingMethod = string.Empty;

    private int _firmClass = FirmClasses.Class1;
    public int FirmClass
    {
        get => _firmClass;
        set
        {
            if (SetProperty(ref _firmClass , value))
            {
                OnPropertyChanged(nameof(IsClass1));
                OnPropertyChanged(nameof(IsClass2));
            }
        }
    }

    public bool IsClass1 => FirmClass == FirmClasses.Class1;

    public bool IsClass2 => FirmClass == FirmClasses.Class2;

    [ObservableProperty]
    private decimal _otherLoss;

    [ObservableProperty]
    private decimal _exceptionLoss;

    [ObservableProperty]
    private bool _isClosed;

    [ObservableProperty]
    private bool _isActive = true;

    // --- 1. SINIF ALANLARI ---
    private decimal _grossWage;
    public decimal GrossWage
    {
        get => _grossWage;
        set
        {
            if (SetProperty(ref _grossWage , value))
            {
                RecalculateAmounts();
            }
        }
    }

    [ObservableProperty]
    private decimal _netWage;

    private string _vatRate = string.Empty;
    public string VatRate
    {
        get => _vatRate;
        set
        {
            if (SetProperty(ref _vatRate , value))
            {
                RecalculateAmounts();
            }
        }
    }

    private decimal _vatAmount;
    public decimal VatAmount
    {
        get => _vatAmount;
        set
        {
            if (SetProperty(ref _vatAmount , value))
            {
                _isVatAmountOverridden = true;
                OnPropertyChanged();
            }
        }
    }

    private decimal _storageRate;
    public decimal StorageRate
    {
        get => _storageRate;
        set
        {
            if (SetProperty(ref _storageRate , value))
            {
                RecalculateAmounts();
            }
        }
    }

    private decimal _storageAmount;
    public decimal StorageAmount
    {
        get => _storageAmount;
        set
        {
            if (SetProperty(ref _storageAmount , value))
            {
                _isStorageAmountOverridden = true;
                OnPropertyChanged();
            }
        }
    }

    [ObservableProperty]
    private int _journalStartNumber=1;

    [ObservableProperty]
    private string _declarationType = string.Empty;

    [ObservableProperty]
    private string _voucherSortMode = string.Empty;

    [ObservableProperty]
    private bool _renumberVouchers;

    [ObservableProperty]
    private string _voucherNumberLength = string.Empty;

    [ObservableProperty]
    private bool _useForeignCurrency;

    [ObservableProperty]
    private string _systemCurrency = string.Empty;

    [ObservableProperty]
    private string _exchangeRateMode = string.Empty;

    // --- 2. SINIF ALANLARI ---
    [ObservableProperty]
    private string _ledgerType = string.Empty;

    [ObservableProperty]
    private int _incomeStartNumber=1;

    [ObservableProperty]
    private int _expenseStartNumber=1;

    [ObservableProperty]
    private decimal _carryoverVat;

    [ObservableProperty]
    private bool _isVatTaxpayer;

    [ObservableProperty]
    private bool _useDbsStockLedger;

    // --- Lookup listeleri ---
    public ObservableCollection<LookupValue> AccountingMethodList { get; } = [];
    public ObservableCollection<LookupValue> CurrencyList { get; } = [];
    public ObservableCollection<LookupValue> DeclarationTypeList { get; } = [];
    public ObservableCollection<LookupValue> ExchangeRateModeList { get; } = [];
    public ObservableCollection<LookupValue> LedgerTypeList { get; } = [];
    public ObservableCollection<LookupValue> VatRateList { get; } = [];
    public ObservableCollection<LookupValue> VoucherNumberLengthList { get; } = [];
    public ObservableCollection<LookupValue> VoucherSortModeList { get; } = [];

    public ObservableCollection<FirmClassItem> FirmClassList { get; } =
    [
        new FirmClassItem(FirmClasses.Class1 , "1. Sınıf"),
        new FirmClassItem(FirmClasses.Class2 , "2. Sınıf")
    ];

    public string ValidationMessage { get; private set; } = string.Empty;
    #endregion Properties

    #region Operations
    public async Task LoadAsync(Guid companyId , Guid periodId)
    {
        Reset();

        _companyId = companyId;
        _periodId = periodId;

        await LoadLookupsAsync();

        Result<Company> companyResult = await _companyService.GetByIdAsync(companyId);
        if (companyResult.IsFailure)
        {
            _messageService.ShowErrors("Firma yüklenemedi" , companyResult.Errors);
            return;
        }

        _companyFoundationDate = NormalizeFoundationDate(companyResult.Data.FoundationDate);

        Result<IReadOnlyList<Period>> periodsResult = await _periodService.GetByCompanyIdAsync(companyId);
        if (periodsResult.IsFailure)
        {
            _messageService.ShowErrors("Dönem listesi yüklenemedi" , periodsResult.Errors);
            return;
        }

        _existingPeriods = periodsResult.Data;

        PopulateYears();

        if (periodId == Guid.Empty)
        {
            Title = "Dönem Ekle";
            IsEditMode = false;
            InitializeForNew();
        }
        else
        {
            Title = "Dönem Düzenle";
            IsEditMode = true;

            Period? existing = _existingPeriods.FirstOrDefault(p => p.Id == periodId);
            if (existing is null)
            {
                _messageService.ShowWarning("Dönem bulunamadı.");
                return;
            }

            FillFromPeriod(existing);
        }

        _isInitializing = false;
        RecalculateDates();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        string? validationError = Validate();
        if (validationError is not null)
        {
            _messageService.ShowWarning(validationError);
            return;
        }

        IsBusy = true;

        try
        {
            Period period = BuildPeriodFromForm();

            Result result;

            if (_periodId == Guid.Empty)
            {
                Result<Period> createResult = await _periodService.CreateAsync(period);
                result = createResult.IsSuccess
                    ? Result.Success()
                    : Result.Failure(createResult.Message , createResult.Status);
            }
            else
            {
                result = await _periodService.UpdateAsync(period);
            }

            if (result.IsFailure)
            {
                _messageService.ShowErrors("Dönem kaydedilemedi" , result.Errors);
                return;
            }

            _messageService.ShowSuccess("Dönem kaydedildi.");
            SaveCompleted.Invoke(this , EventArgs.Empty);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        Cancelled.Invoke(this , EventArgs.Empty);
    }
    #endregion Operations

    #region Helpers
    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotBusy));
    }

    private void Reset()
    {
        _companyId = Guid.Empty;
        _periodId = Guid.Empty;
        _companyFoundationDate = DateTime.MinValue;
        _existingPeriods = [];
        _isVatAmountOverridden = false;
        _isStorageAmountOverridden = false;
        _isInitializing = true;

        Title = "Dönem Ekle";
        IsEditMode = false;

        AvailableYears.Clear();
        AvailableMonths.Clear();

        _suppressSpecialConfirm = true;

        SelectedYear = 0;
        SelectedMonth = null!;
        IsSpecialPeriod = false;

        _suppressSpecialConfirm = false;

        StartDate = DateTime.MinValue;
        EndDate = DateTime.MinValue;

        FinalizationDate = DateTime.MinValue;
        AccountingMethod = string.Empty;
        FirmClass = FirmClasses.Class1;
        OtherLoss = 0m;
        ExceptionLoss = 0m;
        IsClosed = false;
        IsActive = true;

        GrossWage = 0m;
        NetWage = 0m;
        VatRate = string.Empty;
        VatAmount = 0m;
        StorageRate = 0m;
        StorageAmount = 0m;
        JournalStartNumber = 0;
        DeclarationType = string.Empty;
        VoucherSortMode = string.Empty;
        RenumberVouchers = false;
        VoucherNumberLength = string.Empty;
        UseForeignCurrency = false;
        SystemCurrency = string.Empty;
        ExchangeRateMode = string.Empty;

        LedgerType = string.Empty;
        IncomeStartNumber = 0;
        ExpenseStartNumber = 0;
        CarryoverVat = 0m;
        IsVatTaxpayer = false;
        UseDbsStockLedger = false;

        ValidationMessage = string.Empty;
        OnPropertyChanged(nameof(ValidationMessage));
    }

    private async Task LoadLookupsAsync()
    {
        (string Type, ObservableCollection<LookupValue> Target)[] lookups =
        [
            (LookupTypes.AccountingMethod , AccountingMethodList),
            (LookupTypes.Currency , CurrencyList),
            (LookupTypes.DeclarationType , DeclarationTypeList),
            (LookupTypes.ExchangeRateMode , ExchangeRateModeList),
            (LookupTypes.LedgerType , LedgerTypeList),
            (LookupTypes.VatRate , VatRateList),
            (LookupTypes.VoucherNumberLength , VoucherNumberLengthList),
            (LookupTypes.VoucherSortMode , VoucherSortModeList),
            (LookupTypes.MonthName , AvailableMonths)
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

        IEnumerable<LookupValue> sorted = type == LookupTypes.MonthName
            ? result.Data.OrderBy(x => int.TryParse(x.Code , out int n) ? n : int.MaxValue)
            : result.Data.OrderBy(x => x.Name , StringComparer.Create(TurkishCulture , true));

        foreach (LookupValue item in sorted)
        {
            target.Add(item);
        }
    }

    private void PopulateYears()
    {
        int foundationYear = _companyFoundationDate.Year;
        int currentYear = _clock.Now.Year;

        for (int year = foundationYear ; year <= currentYear ; year++)
        {
            AvailableYears.Add(year);
        }
    }

    private void InitializeForNew()
    {
        if (AvailableYears.Count == 0 || AvailableMonths.Count == 0)
        {
            return;
        }

        _isInitializing = true;

        int defaultYear = ResolveDefaultYear();

        SelectedYear = AvailableYears.Contains(defaultYear) ? defaultYear : AvailableYears[^1];

        SelectedMonth = AvailableMonths.FirstOrDefault(m => ParseMonthNumber(m.Code) == MonthJanuary) ?? AvailableMonths[0];

        _suppressSpecialConfirm = true;
        IsSpecialPeriod = false;
        _suppressSpecialConfirm = false;

        _isInitializing = false;
    }

    private int ResolveDefaultYear()
    {
        if (_existingPeriods.Count == 0)
        {
            return _companyFoundationDate.Year;
        }

        int maxYear = _existingPeriods.Max(p => p.Year);
        return maxYear + 1;
    }

    private void FillFromPeriod(Period period)
    {
        _isInitializing = true;

        SelectedYear = period.Year;
        SelectedMonth = AvailableMonths.FirstOrDefault(m => ParseMonthNumber(m.Code) == period.MonthNumber) ?? AvailableMonths[0];

        _suppressSpecialConfirm = true;
        IsSpecialPeriod = period.IsSpecial;
        _suppressSpecialConfirm = false;

        FinalizationDate = period.FinalizationDate;
        AccountingMethod = period.AccountingMethod;
        FirmClass = period.FirmClass;
        OtherLoss = period.OtherLoss;
        ExceptionLoss = period.ExceptionLoss;
        IsClosed = period.IsClosed;
        IsActive = period.IsActive;

        GrossWage = period.GrossWage;
        NetWage = period.NetWage;
        VatRate = period.VatRate;
        VatAmount = period.VatAmount;
        StorageRate = period.StorageRate;
        StorageAmount = period.StorageAmount;
        JournalStartNumber = period.JournalStartNumber;
        DeclarationType = period.DeclarationType;
        VoucherSortMode = period.VoucherSortMode;
        RenumberVouchers = period.RenumberVouchers;
        VoucherNumberLength = period.VoucherNumberLength;
        UseForeignCurrency = period.UseForeignCurrency;
        SystemCurrency = period.SystemCurrency;
        ExchangeRateMode = period.ExchangeRateMode;

        LedgerType = period.LedgerType;
        IncomeStartNumber = period.IncomeStartNumber;
        ExpenseStartNumber = period.ExpenseStartNumber;
        CarryoverVat = period.CarryoverVat;
        IsVatTaxpayer = period.IsVatTaxpayer;
        UseDbsStockLedger = period.UseDbsStockLedger;

        _isInitializing = false;
    }

    private void RecalculateDates()
    {
        if (SelectedYear <= 0 || SelectedMonth is null || AvailableMonths.Count == 0)
        {
            StartDate = DateTime.MinValue;
            EndDate = DateTime.MinValue;
            return;
        }

        int monthNumber = IsSpecialPeriod ? ParseMonthNumber(SelectedMonth.Code) : MonthJanuary;

        if (monthNumber < 1 || monthNumber > 12)
        {
            StartDate = DateTime.MinValue;
            EndDate = DateTime.MinValue;
            return;
        }

        DateTime startDate = new(SelectedYear , monthNumber , 1);

        bool isFirstPeriod = _existingPeriods.Count == 0 || SelectedYear == _companyFoundationDate.Year;

        if (isFirstPeriod && startDate < _companyFoundationDate)
        {
            startDate = _companyFoundationDate;
        }

        DateTime endDate;

        if (IsSpecialPeriod)
        {
            endDate = new DateTime(SelectedYear , monthNumber , 1).AddMonths(PeriodMonthCount).AddDays(-1);
        }
        else
        {
            endDate = new DateTime(SelectedYear , 12 , 31);
        }

        StartDate = startDate;
        EndDate = endDate;
    }

    private void RecalculateAmounts()
    {
        if (!_isVatAmountOverridden && !string.IsNullOrWhiteSpace(VatRate))
        {
            if (decimal.TryParse(VatRate , NumberStyles.Number , TurkishCulture , out decimal vatRateValue))
            {
                VatAmount = Math.Round(GrossWage * vatRateValue / 100m , 2);
            }
            else if (decimal.TryParse(VatRate , NumberStyles.Number , CultureInfo.InvariantCulture , out decimal vatRateInvariant))
            {
                VatAmount = Math.Round(GrossWage * vatRateInvariant / 100m , 2);
            }
        }

        if (!_isStorageAmountOverridden)
        {
            StorageAmount = Math.Round(GrossWage * StorageRate / 100m , 2);
        }
    }

    private string? Validate()
    {
        if (SelectedYear <= 0)
        {
            return "Yıl zorunludur.";
        }

        if (StartDate == DateTime.MinValue || EndDate == DateTime.MinValue)
        {
            return "Başlangıç ve bitiş tarihi hesaplanamadı.";
        }

        if (EndDate <= StartDate)
        {
            return "Bitiş tarihi başlangıç tarihinden sonra olmalıdır.";
        }

        if (IsSpecialPeriod)
        {
            int monthNumber = ParseMonthNumber(SelectedMonth?.Code ?? string.Empty);

            if (monthNumber == MonthJanuary)
            {
                return "Özel dönem için Ocak ayı seçilemez. Lütfen farklı bir başlangıç ayı seçin veya özel dönemi iptal edin.";
            }
        }

        if (_existingPeriods.Count > 0 && !IsEditMode)
        {
            DateTime lastEnd = _existingPeriods.Max(p => p.EndDate).Date;

            if (StartDate.Date <= lastEnd)
            {
                DateTime expected = lastEnd.AddDays(1);
                return $"Önceki dönem {lastEnd:dd.MM.yyyy} tarihinde bitiyor. Yeni dönem en erken {expected:dd.MM.yyyy} tarihinde başlayabilir.";
            }
        }

        if (StartDate.Date < _companyFoundationDate.Date)
        {
            return $"Başlangıç tarihi kuruluş tarihinden ({_companyFoundationDate:dd.MM.yyyy}) önce olamaz.";
        }

        if (FirmClass != FirmClasses.Class1 && FirmClass != FirmClasses.Class2)
        {
            return "Firma sınıfı 1 veya 2 olmalıdır.";
        }

        if (FinalizationDate != DateTime.MinValue && FinalizationDate.Date < _companyFoundationDate.Date)
        {
            return $"Kapanış tarihi kuruluş tarihinden ({_companyFoundationDate:dd.MM.yyyy}) önce olamaz.";
        }

        return null;
    }

    private Period BuildPeriodFromForm()
    {
        Period period = new()
        {
            Id = _periodId == Guid.Empty ? Guid.NewGuid() : _periodId,
            CompanyId = _companyId,
            Year = SelectedYear,
            MonthNumber = IsSpecialPeriod ? ParseMonthNumber(SelectedMonth.Code) : MonthJanuary,
            StartDate = StartDate,
            EndDate = EndDate,
            IsClosed = IsClosed,
            IsActive = IsActive,
            IsSpecial = IsSpecialPeriod,
            FinalizationDate = FinalizationDate,
            AccountingMethod = AccountingMethod,
            FirmClass = FirmClass,
            OtherLoss = OtherLoss,
            ExceptionLoss = ExceptionLoss,
            GrossWage = GrossWage,
            NetWage = NetWage,
            VatRate = VatRate,
            VatAmount = VatAmount,
            StorageRate = StorageRate,
            StorageAmount = StorageAmount,
            JournalStartNumber = JournalStartNumber,
            DeclarationType = DeclarationType,
            VoucherSortMode = VoucherSortMode,
            RenumberVouchers = RenumberVouchers,
            VoucherNumberLength = VoucherNumberLength,
            UseForeignCurrency = UseForeignCurrency,
            SystemCurrency = SystemCurrency,
            ExchangeRateMode = ExchangeRateMode,
            LedgerType = LedgerType,
            IncomeStartNumber = IncomeStartNumber,
            ExpenseStartNumber = ExpenseStartNumber,
            CarryoverVat = CarryoverVat,
            IsVatTaxpayer = IsVatTaxpayer,
            UseDbsStockLedger = UseDbsStockLedger
        };

        return period;
    }

    private static DateTime NormalizeFoundationDate(DateTime foundationDate)
    {
        if (foundationDate == DateTime.MinValue || foundationDate.Year < FallbackFoundationYear)
        {
            return new DateTime(FallbackFoundationYear , 1 , 1);
        }

        return foundationDate.Date;
    }

    private static int ParseMonthNumber(string code)
    {
        return int.TryParse(code , NumberStyles.Integer , TurkishCulture , out int value) ? value : 0;
    }
    #endregion Helpers
}