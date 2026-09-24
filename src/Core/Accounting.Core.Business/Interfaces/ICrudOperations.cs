using Accounting.Core.Domain.Common;
using Accounting.Shared.Results;

namespace Accounting.Core.Business.Interfaces;

public interface ICrudOperations<TEntity> : IEntityOperations<TEntity> where TEntity : BaseEntity
{
    #region Operations
    Task<Result> UpdateAsync(TEntity entity , CancellationToken cancellationToken = default);
    #endregion Operations
}