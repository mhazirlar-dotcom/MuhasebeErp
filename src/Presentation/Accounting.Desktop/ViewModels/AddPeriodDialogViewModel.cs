using Accounting.Core.Domain.Constants;
using Accounting.Core.Domain.Entities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Accounting.Desktop.ViewModels;

public partial class AddPeriodDialogViewModel : ObservableObject
{
    #region Constants
    private const int FallbackFoundationYear = 1900;
    private const int PeriodMonthCount = 12;
    private static readonly CultureInfo TurkishCulture = new("tr-TR");
    #endregion Constants

    #region Fields
    private readonly DateTime _companyFoundationDate;
    private readonly IReadOnlyList<Period> _existingPeriods;
    private bool _isInitializing = true;
    #endregion Fields

    #region Constructor
    public AddPeriodDialogViewModel(DateTime companyFoundationDate , IReadOnlyList<Period> existingPeriods , IReadOnlyList<LookupValue> monthNames)
    {
        _companyFoundationDate = NormalizeFoundationDate(companyFoundationDate);
        _existingPeriods = existingPeriods;

        PopulateMonths(monthNames);
        PopulateYears();

        InitializeDefaultSelection();

        _isInitializing = false;
        Recalculate();
    }
    #endregion Constructor

    #region Events
    public event EventHandler Confirmed = delegate { };
    public event EventHandler Cancelled = delegate { };
    #endregion Events

    #region Properties
    public ObservableCollection<LookupValue> AvailableMonths { get; } = [];

    public ObservableCollection<int> AvailableYears { get; } = [];

    private LookupValue _selectedMonth = null!;
    public LookupValue SelectedMonth
    {
        get => _selectedMonth;
        set
        {
            if (SetProperty(ref _selectedMonth , value ?? null!) && !_isInitializing)
            {
                Recalculate();
            }
        }
    }

    private int _selectedYear;
    public int SelectedYear
    {
        get => _selectedYear;
        set
        {
            if (SetProperty(ref _selectedYear , value) && !_isInitializing)
            {
                Recalculate();
            }
        }
    }

    [ObservableProperty]
    private string _previewText = string.Empty;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    [ObservableProperty]
    private bool _isValid;

    public Period? Result { get; private set; }
    #endregion Properties

    #region Commands
    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private void Confirm()
    {
        if (!IsValid)
        {
            return;
        }

        Result = BuildPeriod();
        Confirmed.Invoke(this , EventArgs.Empty);
    }

    private bool CanConfirm()
    {
        return IsValid;
    }

    [RelayCommand]
    private void Cancel()
    {
        Cancelled.Invoke(this , EventArgs.Empty);
    }
    #endregion Commands

    #region Helpers
    private static DateTime NormalizeFoundationDate(DateTime foundationDate)
    {
        if (foundationDate == DateTime.MinValue || foundationDate.Year < FallbackFoundationYear)
        {
            return new DateTime(FallbackFoundationYear , 1 , 1);
        }

        return foundationDate.Date;
    }

    private void PopulateMonths(IReadOnlyList<LookupValue> monthNames)
    {
        foreach (LookupValue month in monthNames.OrderBy(m => ParseMonthNumber(m.Code)))
        {
            AvailableMonths.Add(month);
        }
    }

    private void PopulateYears()
    {
        int foundationYear = _companyFoundationDate.Year;
        int currentYear = DateTime.Today.Year;

        for (int year = foundationYear ; year <= currentYear ; year++)
        {
            AvailableYears.Add(year);
        }
    }

    private void InitializeDefaultSelection()
    {
        if (AvailableMonths.Count == 0 || AvailableYears.Count == 0)
        {
            return;
        }

        (int defaultMonthNumber , int defaultYear) = ResolveDefaultSelection();

        SelectedMonth = AvailableMonths.FirstOrDefault(m => ParseMonthNumber(m.Code) == defaultMonthNumber)
            ?? AvailableMonths[0];

        SelectedYear = AvailableYears.Contains(defaultYear)
            ? defaultYear
            : AvailableYears[^1];
    }

    private (int MonthNumber , int Year) ResolveDefaultSelection()
    {
        if (_existingPeriods.Count == 0)
        {
            DateTime today = DateTime.Today;
            DateTime baseDate = today < _companyFoundationDate ? _companyFoundationDate : today;
            return (baseDate.Month , baseDate.Year);
        }

        DateTime nextStart = _existingPeriods.Max(p => p.EndDate).Date.AddDays(1);
        return (nextStart.Month , nextStart.Year);
    }

    private void Recalculate()
    {
        if (SelectedMonth is null || SelectedYear <= 0)
        {
            PreviewText = string.Empty;
            ValidationMessage = "Ay ve yıl seçin.";
            SetIsValid(false);
            return;
        }

        int monthNumber = ParseMonthNumber(SelectedMonth.Code);

        if (monthNumber < 1 || monthNumber > 12)
        {
            PreviewText = string.Empty;
            ValidationMessage = "Geçersiz ay seçimi.";
            SetIsValid(false);
            return;
        }

        DateTime newStart = new(SelectedYear , monthNumber , 1);
        DateTime newEnd = newStart.AddMonths(PeriodMonthCount).AddDays(-1);

        PreviewText = $"{newStart:dd.MM.yyyy} - {newEnd:dd.MM.yyyy}";

        string? error = Validate(newStart);

        if (error is not null)
        {
            ValidationMessage = error;
            SetIsValid(false);
            return;
        }

        ValidationMessage = string.Empty;
        SetIsValid(true);
    }

    private string? Validate(DateTime newStart)
    {
        if (newStart < _companyFoundationDate)
        {
            return $"Kuruluş tarihinden ({_companyFoundationDate:dd.MM.yyyy}) önce dönem açılamaz.";
        }

        if (newStart.Year > DateTime.Today.Year)
        {
            return $"İçinde bulunduğumuz yıldan ({DateTime.Today.Year}) sonrası için dönem açılamaz.";
        }

        bool conflict = _existingPeriods.Any(p => p.StartDate.Date == newStart);
        if (conflict)
        {
            return "Bu başlangıç tarihiyle zaten bir dönem mevcut.";
        }

        if (_existingPeriods.Count > 0)
        {
            DateTime lastEnd = _existingPeriods.Max(p => p.EndDate).Date;
            DateTime expectedStart = lastEnd.AddDays(1);

            if (newStart != expectedStart)
            {
                return $"Önceki dönem {lastEnd:dd.MM.yyyy} tarihinde bitiyor. Yeni dönem en erken {expectedStart:dd.MM.yyyy} tarihinde başlayabilir.";
            }
        }

        return null;
    }

    private Period BuildPeriod()
    {
        int monthNumber = ParseMonthNumber(SelectedMonth.Code);
        DateTime startDate = new(SelectedYear , monthNumber , 1);
        DateTime endDate = startDate.AddMonths(PeriodMonthCount).AddDays(-1);

        return new Period
        {
            Year = SelectedYear ,
            MonthNumber = monthNumber ,
            StartDate = startDate ,
            EndDate = endDate ,
            IsClosed = false ,
            IsActive = true ,
            FirmClass = FirmClasses.Class1 ,
            OtherLoss = 0m ,
            ExceptionLoss = 0m
        };
    }

    private void SetIsValid(bool value)
    {
        if (IsValid == value)
        {
            return;
        }

        IsValid = value;
        ConfirmCommand.NotifyCanExecuteChanged();
    }

    private static int ParseMonthNumber(string code)
    {
        return int.TryParse(code , NumberStyles.Integer , TurkishCulture , out int value) ? value : 0;
    }
    #endregion Helpers
}