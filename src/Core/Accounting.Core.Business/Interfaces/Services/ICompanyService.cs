using Accounting.Core.Domain.Entities;
using Accounting.Shared.Results;

namespace Accounting.Core.Business.Interfaces.Services;

public interface ICompanyService : ICrudOperations<Company>
{
    #region Operations
    Task<Result<IReadOnlyList<Company>>> GetByUserIdAsync(bool includeActive = true , bool includePassive = false , CancellationToken cancellationToken = default);

    Task<Result> SetActiveAsync(Guid id , bool isActive , CancellationToken cancellationToken = default);
    #endregion Operations
}