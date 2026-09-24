using Accounting.Core.Domain.Common;
using Accounting.Shared.Results;
using System.Linq.Expressions;

namespace Accounting.Core.Business.Interfaces.Repositories;

public interface IRepository<TEntity> : IEntityOperations<TEntity> where TEntity : BaseEntity
{
    #region Operations
    Task<Result<TEntity>> GetByIdForUpdateAsync(Guid id , CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<TEntity>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<TEntity>>> GetAllAsync(Expression<Func<TEntity , bool>> filter , CancellationToken cancellationToken = default);

    Task<Result<bool>> HasAnyAsync(CancellationToken cancellationToken = default);

    Task<Result<bool>> HasReferencesAsync(Guid id , CancellationToken cancellationToken = default);

    Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default);
    #endregion Operations
}