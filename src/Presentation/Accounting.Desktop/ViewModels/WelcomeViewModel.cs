using Accounting.Core.Business.Interfaces;
using Accounting.Core.Business.Interfaces.CrossCutting;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Markers;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Accounting.Desktop.ViewModels;

public partial class WelcomeViewModel(ICurrentUser currentUser , ITenantContext tenantContext) : ObservableObject, ITransientService
{
    #region Properties
    [ObservableProperty]
    private string _welcomeMessage = string.Empty;

    [ObservableProperty]
    private string _companyPeriodInfo = string.Empty;

    [ObservableProperty]
    private bool _isCompanyInfoVisible;
    #endregion Properties

    #region Operations
    public void Load()
    {
        WelcomeMessage = $"Hoş geldiniz, {currentUser.UserName}";

        if (!tenantContext.HasCompany)
        {
            CompanyPeriodInfo = string.Empty;
            IsCompanyInfoVisible = false;
            return;
        }

        Company company = tenantContext.CurrentCompany;

        if (!tenantContext.HasPeriod)
        {
            CompanyPeriodInfo = $"{company.Name} firması seçili. Dönem seçilmedi.";
            IsCompanyInfoVisible = true;
            return;
        }

        Period period = tenantContext.CurrentPeriod;

        CompanyPeriodInfo = $"{company.Name} firmasının {period.StartDate:dd.MM.yyyy} - {period.EndDate:dd.MM.yyyy} döneminde çalışıyorsunuz.";
        IsCompanyInfoVisible = true;
    }
    #endregion Operations
}