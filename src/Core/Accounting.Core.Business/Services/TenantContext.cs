using Accounting.Core.Business.Interfaces.CrossCutting;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Markers;

namespace Accounting.Core.Business.Services;

public sealed class TenantContext : ITenantContext, ILocalSingletonService
{
    #region Constructor
    public TenantContext()
    {
        CurrentCompany = CreateEmptyCompany();
        CurrentPeriod = CreateEmptyPeriod();
    }
    #endregion Constructor

    #region Properties
    public Company CurrentCompany { get; private set; }
    public Guid CurrentCompanyId => CurrentCompany.Id;
    public bool HasCompany => CurrentCompany.Id != Guid.Empty;

    public Period CurrentPeriod { get; private set; }
    public Guid CurrentPeriodId => CurrentPeriod.Id;
    public bool HasPeriod => CurrentPeriod.Id != Guid.Empty;
    #endregion Properties

    #region Operations
    public void SetCompany(Company company)
    {
        CurrentCompany = company;
        CurrentPeriod = CreateEmptyPeriod();
    }

    public void SetPeriod(Period period)
    {
        CurrentPeriod = period;
    }

    public void ClearPeriod()
    {
        CurrentPeriod = CreateEmptyPeriod();
    }

    public void Clear()
    {
        CurrentCompany = CreateEmptyCompany();
        CurrentPeriod = CreateEmptyPeriod();
    }
    #endregion Operations

    #region Helpers
    private static Company CreateEmptyCompany()
    {
        return new Company { Id = Guid.Empty };
    }

    private static Period CreateEmptyPeriod()
    {
        return new Period { Id = Guid.Empty };
    }
    #endregion Helpers
}