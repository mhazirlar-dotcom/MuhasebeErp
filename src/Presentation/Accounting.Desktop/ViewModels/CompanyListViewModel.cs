using Accounting.Core.Business.Interfaces.CrossCutting;
using Accounting.Core.Business.Interfaces.Services;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Markers;
using Accounting.Shared.Results;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Accounting.Desktop.ViewModels;

public partial class CompanyListViewModel(ICompanyService companyService , IMessageService messageService) : ObservableObject, ITransientService
{
    #region Constants
    public const string FilterAll = "Tümü";
    public const string FilterActive = "Aktif";
    public const string FilterPassive = "Pasif";
    #endregion Constants

    #region Events
    public event EventHandler NewCompanyRequested = delegate { };
    public event EventHandler<Guid> EditCompanyRequested = delegate { };
    public event EventHandler CompaniesChanged = delegate { };
    #endregion Events

    #region Properties
    [ObservableProperty]
    private bool _isBusy;

    private string _filterMode = FilterActive;
    public string FilterMode
    {
        get => _filterMode;
        set
        {
            if (SetProperty(ref _filterMode , value))
            {
                OnPropertyChanged(nameof(IsFilterAll));
                OnPropertyChanged(nameof(IsFilterActive));
                OnPropertyChanged(nameof(IsFilterPassive));
            }
        }
    }

    public bool IsFilterAll => FilterMode == FilterAll;
    public bool IsFilterActive => FilterMode == FilterActive;
    public bool IsFilterPassive => FilterMode == FilterPassive;

    private Company _selectedCompany = new();
    public Company SelectedCompany
    {
        get => _selectedCompany;
        set
        {
            if (SetProperty(ref _selectedCompany , value ?? new Company()))
            {
                OnPropertyChanged(nameof(HasSelection));
                OnPropertyChanged(nameof(CanDeactivate));
                OnPropertyChanged(nameof(CanActivate));
            }
        }
    }

    public bool HasSelection => SelectedCompany is not null && SelectedCompany.Id != Guid.Empty;

    public bool CanDeactivate => HasSelection && SelectedCompany.IsActive;

    public bool CanActivate => HasSelection && !SelectedCompany.IsActive;

    public ObservableCollection<Company> Companies { get; } = [];
    #endregion Properties

    #region Operations
    public async Task LoadAsync()
    {
        IsBusy = true;

        try
        {
            bool includeActive = FilterMode == FilterActive || FilterMode == FilterAll;
            bool includePassive = FilterMode == FilterPassive || FilterMode == FilterAll;

            Result<IReadOnlyList<Company>> result = await companyService.GetByUserIdAsync(includeActive , includePassive);

            if (result.IsFailure)
            {
                messageService.ShowErrors("Firma listesi yüklenemedi" , result.Errors);
                return;
            }

            Companies.Clear();
            foreach (Company company in result.Data)
            {
                Companies.Add(company);
            }

            SelectedCompany = new Company();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SetFilterAsync(string mode)
    {
        if (string.IsNullOrWhiteSpace(mode))
        {
            return;
        }

        if (FilterMode == mode)
        {
            return;
        }

        FilterMode = mode;
        await LoadAsync();
    }

    [RelayCommand]
    private void NewCompany()
    {
        NewCompanyRequested.Invoke(this , EventArgs.Empty);
    }

    [RelayCommand]
    private void OpenDetails()
    {
        if (!HasSelection)
        {
            messageService.ShowWarning("Detay için bir firma seçin.");
            return;
        }

        EditCompanyRequested.Invoke(this , SelectedCompany.Id);
    }

    [RelayCommand]
    private async Task DeactivateCompanyAsync()
    {
        if (!CanDeactivate)
        {
            messageService.ShowWarning("Pasife almak için aktif bir firma seçin.");
            return;
        }

        if (!messageService.Confirm("Pasife Alma Onayı" , $"{SelectedCompany.ShortName} firmasını pasife almak istediğinize emin misiniz?"))
        {
            return;
        }

        await SetActiveInternalAsync(false);
    }

    [RelayCommand]
    private async Task ActivateCompanyAsync()
    {
        if (!CanActivate)
        {
            messageService.ShowWarning("Aktifleştirmek için pasif bir firma seçin.");
            return;
        }

        if (!messageService.Confirm("Aktifleştirme Onayı" , $"{SelectedCompany.ShortName} firmasını tekrar aktif hale getirmek istediğinize emin misiniz?"))
        {
            return;
        }

        await SetActiveInternalAsync(true);
    }
    #endregion Operations

    #region Helpers
    private async Task SetActiveInternalAsync(bool isActive)
    {
        IsBusy = true;

        try
        {
            Result result = await companyService.SetActiveAsync(SelectedCompany.Id , isActive);

            if (result.IsFailure)
            {
                messageService.ShowErrors("İşlem başarısız" , result.Errors);
                return;
            }

            string action = isActive ? "aktif hale getirildi" : "pasife alındı";
            messageService.ShowSuccess($"{SelectedCompany.ShortName} {action}.");
        }
        finally
        {
            IsBusy = false;
        }

        await LoadAsync();

        CompaniesChanged.Invoke(this , EventArgs.Empty);
    }
    #endregion Helpers
}