using Accounting.Core.Business.Interfaces.Repositories;
using Accounting.Core.DataAccess.Extensions;
using Accounting.Core.Domain.Common;
using Accounting.Shared.Markers;
using Accounting.Shared.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Accounting.Core.DataAccess.Repositories;

public abstract class RepositoryBase<TEntity, TContext>(TContext context) : IRepository<TEntity>, IScopedService
    where TEntity : BaseEntity
    where TContext : DbContext
{
    #region Fields
    protected readonly TContext _context = context;
    protected readonly DbSet<TEntity> _set = context.Set<TEntity>();
    #endregion Fields

    #region Operations
    public virtual async Task<Result<TEntity>> GetByIdAsync(Guid id , CancellationToken cancellationToken = default)
    {
        return await _set
            .AsNoTracking()
            .FirstOrNotFoundAsync(x => x.Id == id , $"Kayıt bulunamadı. Id: {id}" , cancellationToken);
    }

    public virtual async Task<Result<TEntity>> GetByIdForUpdateAsync(Guid id , CancellationToken cancellationToken = default)
    {
        return await _set
            .FirstOrNotFoundAsync(x => x.Id == id , $"Kayıt bulunamadı. Id: {id}" , cancellationToken);
    }

    public virtual async Task<Result<IReadOnlyList<TEntity>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await GetAllAsync(x => true , cancellationToken);
    }

    public virtual async Task<Result<IReadOnlyList<TEntity>>> GetAllAsync(
        Expression<Func<TEntity , bool>> filter ,
        CancellationToken cancellationToken = default)
    {
        List<TEntity> list = await _set
            .AsNoTracking()
            .Where(filter)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<TEntity>>.Success(list);
    }

    public virtual async Task<Result<TEntity>> CreateAsync(TEntity entity , CancellationToken cancellationToken = default)
    {
        await _set.AddAsync(entity , cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<TEntity>.Success(entity , "Kayıt eklendi.");
    }

    public virtual async Task<Result> DeleteAsync(Guid id , CancellationToken cancellationToken = default)
    {
        Result<TEntity> entityResult = await GetByIdForUpdateAsync(id, cancellationToken);

        if (entityResult.IsFailure)
        {
            return Result.NotFound(entityResult.Message);
        }

        Result<bool> referencesResult = await HasReferencesAsync(id, cancellationToken);

        if (referencesResult.IsFailure)
        {
            return Result.Failure(referencesResult.Message);
        }

        if (referencesResult.Data)
        {
            return Result.Conflict("Bu kayıt başka yerlerde kullanıldığı için silinemez.");
        }

        _set.Remove(entityResult.Data);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success("Kayıt silindi.");
    }

    public virtual async Task<Result<bool>> HasAnyAsync(CancellationToken cancellationToken = default)
    {
        bool any = await _set.AnyAsync(cancellationToken);
        return Result<bool>.Success(any);
    }

    public virtual async Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success("Değişiklikler kaydedildi.");
    }
    #endregion Operations

    #region Abstract
    public abstract Task<Result<bool>> HasReferencesAsync(Guid id , CancellationToken cancellationToken = default);
    #endregion Abstract
}