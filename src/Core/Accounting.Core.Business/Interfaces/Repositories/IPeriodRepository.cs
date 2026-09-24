using Accounting.Core.Domain.Entities;
using Accounting.Shared.Results;

namespace Accounting.Core.Business.Interfaces.Repositories;

public interface IPeriodRepository : IRepository<Period>
{
    #region Operations
    Task<Result<IReadOnlyList<Period>>> GetByCompanyIdAsync(
        Guid companyId ,
        CancellationToken cancellationToken = default);

    Task<Result<Period>> GetByCompanyAndYearAsync(
        Guid companyId ,
        int year ,
        CancellationToken cancellationToken = default);
    #endregion Operations
}