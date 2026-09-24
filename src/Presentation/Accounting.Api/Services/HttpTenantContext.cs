using Accounting.Api.Extensions;
using Accounting.Core.Business.Interfaces.CrossCutting;
using Accounting.Core.Business.Interfaces.Repositories;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Markers;
using Accounting.Shared.Results;

namespace Accounting.Api.Services;

public sealed class HttpTenantContext(IHttpContextAccessor httpContextAccessor , ICompanyRepository companyRepository , IPeriodRepository periodRepository) : ITenantContext, IScopedService
{
    #region Fields
    private Company? _cachedCompany;
    private Period? _cachedPeriod;
    private bool _companyLoaded;
    private bool _periodLoaded;
    #endregion Fields

    #region Properties
    public Company CurrentCompany
    {
        get
        {
            if (!_companyLoaded)
            {
                _cachedCompany = LoadCompany();
                _companyLoaded = true;
            }

            return _cachedCompany!;
        }
    }

    public Guid CurrentCompanyId => httpContextAccessor.GetCompanyId();

    public bool HasCompany => CurrentCompanyId != Guid.Empty;

    public Period CurrentPeriod
    {
        get
        {
            if (!_periodLoaded)
            {
                _cachedPeriod = LoadPeriod();
                _periodLoaded = true;
            }

            return _cachedPeriod!;
        }
    }

    public Guid CurrentPeriodId => httpContextAccessor.GetPeriodId();

    public bool HasPeriod => CurrentPeriodId != Guid.Empty;
    #endregion Properties

    #region Operations
    public void SetCompany(Company company)
    {
        // API'de tenant, X-Company-Id header'ından gelir; bu metot no-op.
    }

    public void SetPeriod(Period period)
    {
        // API'de tenant, X-Period-Id header'ından gelir; bu metot no-op.
    }

    public void ClearPeriod()
    {
        // API'de tenant, X-Period-Id header'ından gelir; bu metot no-op.
    }

    public void Clear()
    {
        // API'de tenant, header'lardan gelir; bu metot no-op.
    }
    #endregion Operations

    #region Helpers
    private Company LoadCompany()
    {
        Guid companyId = CurrentCompanyId;

        if (companyId == Guid.Empty)
        {
            return CreateEmptyCompany();
        }

        Result<Company> result = companyRepository
            .GetByIdAsync(companyId)
            .GetAwaiter()
            .GetResult();

        return result.IsSuccess ? result.Data : CreateEmptyCompany();
    }

    private Period LoadPeriod()
    {
        Guid periodId = CurrentPeriodId;

        if (periodId == Guid.Empty)
        {
            return CreateEmptyPeriod();
        }

        Result<Period> result = periodRepository
            .GetByIdAsync(periodId)
            .GetAwaiter()
            .GetResult();

        return result.IsSuccess ? result.Data : CreateEmptyPeriod();
    }

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