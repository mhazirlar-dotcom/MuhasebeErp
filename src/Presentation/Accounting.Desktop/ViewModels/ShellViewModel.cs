using Accounting.Core.Business.Interfaces;
using Accounting.Core.Business.Interfaces.CrossCutting;
using Accounting.Core.Business.Interfaces.Services;
using Accounting.Core.Domain.Entities;
using Accounting.Desktop.Interfaces;
using Accounting.Shared.Markers;
using Accounting.Shared.Results;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace Accounting.Desktop.ViewModels;

public partial class ShellViewModel : ObservableObject, IScopedService, IDisposable
{
    #region Events
    public event EventHandler LogoutRequested = delegate { };
    #endregion Events

    #region Fields
    private readonly IServiceProvider _serviceProvider;
    private readonly ICompanyService _companyService;
    private readonly IPeriodService _periodService;
    private readonly ICurrentUser _currentUser;
    private readonly ITenantContext _tenantContext;
    private readonly IMessageService _messageService;
    private readonly ITokenStore _tokenStore;
    private bool _suppressCompanyChanged = false;
    private bool _disposed = false;

    private Guid _currentCompanyId = Guid.Empty;
    #endregion Fields

    #region Constructor
    public ShellViewModel(IServiceProvider serviceProvider , ICompanyService companyService , IPeriodService periodService , ICurrentUser currentUser , ITenantContext tenantContext , IMessageService messageService , ITokenStore tokenStore)
    {
        _serviceProvider = serviceProvider;
        _companyService = companyService;
        _periodService = periodService;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
        _messageService = messageService;
        _tokenStore = tokenStore;

        _tokenStore.LoggedOut += OnTokenStoreLoggedOut;
    }
    #endregion Constructor

    #region Properties
    [ObservableProperty]
    private object _currentView = null!;

    [ObservableProperty]
    private object _footerContent = null!;

    [ObservableProperty]
    private string _userDisplayName = string.Empty;

    private Company _selectedCompany = new();
    public Company SelectedCompany
    {
        get => _selectedCompany;
        set
        {
            if (SetProperty(ref _selectedCompany , value ?? new Company()))
            {
                OnPropertyChanged(nameof(CanConfirmSelection));
                OnSelectedCompanyChanged(_selectedCompany);
            }
        }
    }

    private Period _selectedPeriod = new();
    public Period SelectedPeriod
    {
        get => _selectedPeriod;
        set
        {
            if (SetProperty(ref _selectedPeriod , value ?? new Period()))
            {
                OnPropertyChanged(nameof(CanConfirmSelection));
            }
        }
    }

    [ObservableProperty]
    private bool _hasCompanies;

    [ObservableProperty]
    private bool _hasPeriods;

    public bool CanConfirmSelection =>
        SelectedCompany is not null && SelectedCompany.Id != Guid.Empty
        && SelectedPeriod is not null && SelectedPeriod.Id != Guid.Empty;

    public ObservableCollection<Company> Companies { get; } = [];
    public ObservableCollection<Period> Periods { get; } = [];
    #endregion Properties

    #region Operations
    public async Task LoadAsync()
    {
        await LoadCompaniesAsync();
        ShowEmptyView();

        if (!HasCompanies)
        {
            _messageService.ShowInfo("Yetkili olduğunuz bir firma bulunmuyor. 'Yönetici → Firma İşlemleri → Firma Girişi' ile firma ekleyin.");
        }
    }

    public async Task RefreshCompaniesAsync()
    {
        await LoadCompaniesAsync();
    }

    public void SetFooterContent(object content)
    {
        FooterContent = content;
    }

    public void ClearFooterContent()
    {
        FooterContent = null!;
    }

    public async Task OpenCompanyEditViewAsync(Guid companyId , string? initialSection = null)
    {
        _currentCompanyId = companyId;

        CompanyEditViewModel editViewModel = _serviceProvider.GetRequiredService<CompanyEditViewModel>();
        await editViewModel.LoadAsync(companyId , initialSection);

        Views.CompanyEditView view = new(editViewModel , this);

        view.SaveCompleted += async (_ , _) => await OnCompanyEditSavedAsync();
        view.Cancelled += (_ , _) => OnCompanyEditCancelled();

        CurrentView = view;
    }

    public async Task OpenPeriodEditViewAsync(Guid periodId)
    {
        if (_currentCompanyId == Guid.Empty)
        {
            _messageService.ShowWarning("Önce bir firma seçin.");
            return;
        }

        PeriodEditViewModel periodEditViewModel = _serviceProvider.GetRequiredService<PeriodEditViewModel>();
        await periodEditViewModel.LoadAsync(_currentCompanyId , periodId);

        Views.PeriodEditView view = new(periodEditViewModel , this);

        view.SaveCompleted += async (_ , _) => await OnPeriodEditSavedAsync();
        view.Cancelled += async (_ , _) => await OnPeriodEditCancelledAsync();

        CurrentView = view;
    }

    [RelayCommand]
    private void Navigate(string viewKey)
    {
        if (string.IsNullOrWhiteSpace(viewKey))
        {
            return;
        }

        switch (viewKey)
        {
            case "FirmaGirisi":
                _ = OpenCompanyEditViewAsync(Guid.Empty);
                return;

            case "FirmaListesi":
                ShowCompanyListView();
                return;

            default:
                ShowEmptyView();
                return;
        }
    }

    [RelayCommand]
    private void ConfirmSelection()
    {
        if (!CanConfirmSelection)
        {
            _messageService.ShowWarning("Lütfen bir firma ve dönem seçin.");
            return;
        }

        ApplySelectedCompanyAndPeriod();
    }

    [RelayCommand]
    private void Logout()
    {
        if (!_messageService.Confirm("Çıkış Onayı" , "Çıkış yapmak istediğinize emin misiniz?"))
        {
            return;
        }

        _currentUser.Clear();
        _tenantContext.Clear();
        _tokenStore.Clear();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _tokenStore.LoggedOut -= OnTokenStoreLoggedOut;
        _disposed = true;

        GC.SuppressFinalize(this);
    }
    #endregion Operations

    #region Helpers
    private async void OnSelectedCompanyChanged(Company company)
    {
        if (_suppressCompanyChanged)
        {
            return;
        }

        await LoadPeriodsAsync(company);
    }

    private async Task LoadCompaniesAsync()
    {
        UserDisplayName = _currentUser.UserName;

        Result<IReadOnlyList<Company>> result = await _companyService.GetByUserIdAsync();

        if (result.IsFailure)
        {
            _messageService.ShowErrors("Firma listesi yüklenemedi" , result.Errors);
            return;
        }

        _suppressCompanyChanged = true;

        Companies.Clear();
        foreach (Company company in result.Data)
        {
            Companies.Add(company);
        }

        HasCompanies = Companies.Count > 0;

        SelectedCompany = new Company();
        Periods.Clear();
        SelectedPeriod = new Period();
        HasPeriods = false;

        _suppressCompanyChanged = false;
    }

    private async Task LoadPeriodsAsync(Company company)
    {
        Periods.Clear();
        SelectedPeriod = new Period();
        HasPeriods = false;

        if (company is null || company.Id == Guid.Empty)
        {
            return;
        }

        Result<IReadOnlyList<Period>> result = await _periodService.GetByCompanyIdAsync(company.Id);

        if (result.IsFailure)
        {
            _messageService.ShowErrors("Dönem listesi yüklenemedi" , result.Errors);
            return;
        }

        foreach (Period period in result.Data)
        {
            Periods.Add(period);
        }

        HasPeriods = Periods.Count > 0;
    }

    private async Task ReloadPeriodsForCurrentCompanyAsync()
    {
        if (_currentCompanyId == Guid.Empty)
        {
            return;
        }

        Company? company = Companies.FirstOrDefault(c => c.Id == _currentCompanyId);

        if (company is null)
        {
            // Firma listede yoksa (yeni kaydedilmiş olabilir) listeyi tazele.
            await LoadCompaniesAsync();

            company = Companies.FirstOrDefault(c => c.Id == _currentCompanyId);

            if (company is null)
            {
                return;
            }
        }

        await LoadPeriodsAsync(company);

        // SelectedCompany referansını listedeki gerçek instance ile eşleştir.
        if (SelectedCompany?.Id != company.Id)
        {
            _suppressCompanyChanged = true;
            SelectedCompany = company;
            _suppressCompanyChanged = false;
        }

        // En son eklenen (en yüksek yıl) dönemi otomatik seç.
        Period? lastPeriod = Periods
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.MonthNumber)
            .FirstOrDefault();

        if (lastPeriod is not null)
        {
            SelectedPeriod = lastPeriod;
        }
    }

    private void ApplySelectedCompanyAndPeriod()
    {
        if (!CanConfirmSelection)
        {
            return;
        }

        _tenantContext.SetCompany(SelectedCompany);
        _tenantContext.SetPeriod(SelectedPeriod);

        Views.WelcomeView view = _serviceProvider.GetRequiredService<Views.WelcomeView>();
        CurrentView = view;
    }

    private void ShowEmptyView()
    {
        ClearFooterContent();
        CurrentView = new Views.EmptyView();
    }

    private async void ShowCompanyListView()
    {
        ClearFooterContent();

        CompanyListViewModel listViewModel = _serviceProvider.GetRequiredService<CompanyListViewModel>();
        await listViewModel.LoadAsync();

        Views.CompanyListView view = new(listViewModel , this);
        CurrentView = view;
    }

    private async Task OnCompanyEditSavedAsync()
    {
        await LoadCompaniesAsync();
        await ReloadPeriodsForCurrentCompanyAsync();
    }

    private void OnCompanyEditCancelled()
    {
        ShowCompanyListView();
    }

    private async Task OnPeriodEditSavedAsync()
    {
        await ReloadPeriodsForCurrentCompanyAsync();
        await OpenCompanyEditViewAsync(_currentCompanyId , CompanyEditViewModel.SectionDonem);
    }

    private async Task OnPeriodEditCancelledAsync()
    {
        await ReloadPeriodsForCurrentCompanyAsync();
        await OpenCompanyEditViewAsync(_currentCompanyId , CompanyEditViewModel.SectionDonem);
    }

    private void OnTokenStoreLoggedOut(object? sender , EventArgs e)
    {
        LogoutRequested.Invoke(this , EventArgs.Empty);
    }
    #endregion Helpers
}