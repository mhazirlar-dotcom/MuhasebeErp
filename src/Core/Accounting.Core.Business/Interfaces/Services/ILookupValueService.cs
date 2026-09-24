using Accounting.Core.Domain.Entities;
using Accounting.Shared.Results;

namespace Accounting.Core.Business.Interfaces.Services;

public interface ILookupValueService : ICrudOperations<LookupValue>
{
    #region Operations
    Task<Result<IReadOnlyList<LookupValue>>> GetByTypeAsync(string type , bool includeInactive = false , CancellationToken cancellationToken = default);

    Task<Result<LookupValue>> GetByTypeAndCodeAsync(string type , string code , CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<LookupValue>>> GetChildrenAsync(Guid parentId , bool includeInactive = false , CancellationToken cancellationToken = default);
    #endregion Operations
}