using Accounting.Core.Domain.Common;
using Accounting.Shared.Results;

namespace Accounting.Core.Business.Interfaces;

public interface IEntityOperations<TEntity> where TEntity : BaseEntity
{
    #region Operations
    Task<Result<TEntity>> GetByIdAsync(Guid id , CancellationToken cancellationToken = default);

    Task<Result<TEntity>> CreateAsync(TEntity entity , CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid id , CancellationToken cancellationToken = default);
    #endregion Operations
}