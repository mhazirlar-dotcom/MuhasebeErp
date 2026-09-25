using Accounting.Core.Business.Interfaces.CrossCutting;
using Accounting.Core.Business.Interfaces.Services;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Markers;
using Accounting.Shared.Results;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Accounting.Desktop.ViewModels;

public partial class PeriodListViewModel(IPeriodService periodService , IMessageService messageService) : ObservableObject, ITransientService
{
    #region Events
    public event EventHandler AddRequested = delegate { };
    public event EventHandler<Guid> EditRequested = delegate { };
    #endregion Events

    #region Fields
    private Guid _companyId = Guid.Empty;
    #endregion Fields

    #region Properties
    [ObservableProperty]
    private bool _isBusy;

    public bool IsNotBusy => !IsBusy;

    private Period _selectedPeriod = new();
    public Period SelectedPeriod
    {
        get => _selectedPeriod;
        set
        {
            if (SetProperty(ref _selectedPeriod , value ?? new Period()))
            {
                OnPropertyChanged(nameof(HasSelection));
                OnPropertyChanged(nameof(CanEdit));
                OnPropertyChanged(nameof(CanDelete));
            }
        }
    }

    public bool HasSelection => SelectedPeriod is not null && SelectedPeriod.Id != Guid.Empty;

    public bool CanEdit => HasSelection;

    public bool CanDelete => HasSelection && !SelectedPeriod.IsClosed;

    public ObservableCollection<Period> Periods { get; } = [];
    #endregion Properties

    #region Operations
    public async Task LoadAsync(Guid companyId)
    {
        _companyId = companyId;

        IsBusy = true;

        try
        {
            Result<IReadOnlyList<Period>> result = await periodService.GetByCompanyIdAsync(companyId);

            if (result.IsFailure)
            {
                messageService.ShowErrors("Dönem listesi yüklenemedi" , result.Errors);
                return;
            }

            Periods.Clear();
            foreach (Period period in result.Data.OrderBy(p => p.Year))
            {
                Periods.Add(period);
            }

            SelectedPeriod = new Period();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Add()
    {
        AddRequested.Invoke(this , EventArgs.Empty);
    }

    [RelayCommand]
    private void Edit()
    {
        if (!HasSelection)
        {
            messageService.ShowWarning("Düzenlemek için bir dönem seçin.");
            return;
        }

        EditRequested.Invoke(this , SelectedPeriod.Id);
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (!HasSelection)
        {
            messageService.ShowWarning("Silmek için bir dönem seçin.");
            return;
        }

        if (SelectedPeriod.IsClosed)
        {
            messageService.ShowWarning("Kapanmış dönem silinemez.");
            return;
        }

        bool confirmed = messageService.Confirm(
            "Dönem Silme Onayı" ,
            $"{SelectedPeriod.Year} yılı dönemini silmek istediğinize emin misiniz?");

        if (!confirmed)
        {
            return;
        }

        IsBusy = true;

        try
        {
            Result result = await periodService.DeleteAsync(SelectedPeriod.Id);

            if (result.IsFailure)
            {
                messageService.ShowErrors("Dönem silinemedi" , result.Errors);
                return;
            }

            messageService.ShowSuccess("Dönem silindi.");
        }
        finally
        {
            IsBusy = false;
        }

        await LoadAsync(_companyId);
    }

    public async Task RefreshAsync()
    {
        await LoadAsync(_companyId);
    }
    #endregion Operations

    #region Helpers
    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotBusy));
    }
    #endregion Helpers
}