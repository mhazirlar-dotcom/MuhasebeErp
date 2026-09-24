using Accounting.Core.Domain.Entities;
using Accounting.Shared.Results;

namespace Accounting.Core.Business.Interfaces.Services;

public interface IPeriodService : ICrudOperations<Period>
{
    #region Operations
    Task<Result<IReadOnlyList<Period>>> GetByCompanyIdAsync(
        Guid companyId ,
        CancellationToken cancellationToken = default);
    #endregion Operations
}