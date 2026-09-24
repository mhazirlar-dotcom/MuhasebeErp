using Accounting.Core.Domain.Entities;

namespace Accounting.Core.Business.Interfaces.CrossCutting;

public interface ITenantContext
{
    #region Properties
    Company CurrentCompany { get; }
    Guid CurrentCompanyId { get; }
    bool HasCompany { get; }

    Period CurrentPeriod { get; }
    Guid CurrentPeriodId { get; }
    bool HasPeriod { get; }
    #endregion Properties

    #region Operations
    void SetCompany(Company company);
    void SetPeriod(Period period);
    void ClearPeriod();
    void Clear();
    #endregion Operations
}