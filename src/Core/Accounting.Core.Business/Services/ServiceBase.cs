using Accounting.Core.Business.Extensions;
using Accounting.Core.Business.Interfaces;
using Accounting.Core.Business.Interfaces.Repositories;
using Accounting.Core.Domain.Common;
using Accounting.Shared.Markers;
using Accounting.Shared.Results;
using FluentValidation;

namespace Accounting.Core.Business.Services;

public abstract class ServiceBase<TEntity, TRepository>(TRepository repository , IValidator<TEntity>? validator = null)
    : ICrudOperations<TEntity>, IScopedService
    where TEntity : BaseEntity
    where TRepository : IRepository<TEntity>
{
    #region Fields
    protected readonly TRepository _repository = repository;
    private readonly IValidator<TEntity>? _validator = validator;
    #endregion Fields

    #region Operations
    public virtual Task<Result<TEntity>> GetByIdAsync(Guid id , CancellationToken cancellationToken = default)
    {
        return _repository.GetByIdAsync(id , cancellationToken);
    }

    public virtual async Task<Result<TEntity>> CreateAsync(TEntity entity , CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Error>? errors = await ValidateAsync(entity , cancellationToken);

        if (errors is not null)
        {
            return Result<TEntity>.ValidationFailure(errors);
        }

        return await _repository.CreateAsync(entity , cancellationToken);
    }

    public virtual async Task<Result> UpdateAsync(TEntity entity , CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Error>? errors = await ValidateAsync(entity , cancellationToken);

        if (errors is not null)
        {
            return Result.ValidationFailure(errors);
        }

        Result<TEntity> existingResult = await _repository.GetByIdForUpdateAsync(entity.Id , cancellationToken);

        if (existingResult.IsFailure)
        {
            return Result.NotFound(existingResult.Message);
        }

        CopyFields(existingResult.Data , entity);

        return await _repository.SaveChangesAsync(cancellationToken);
    }

    public virtual Task<Result> DeleteAsync(Guid id , CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(id , cancellationToken);
    }
    #endregion Operations

    #region Helpers
    protected Task<IReadOnlyList<Error>?> ValidateAsync(TEntity entity , CancellationToken cancellationToken)
    {
        if (_validator is null)
        {
            return Task.FromResult<IReadOnlyList<Error>?>(null);
        }

        return entity.ValidateErrorsAsync(_validator , cancellationToken);
    }
    #endregion Helpers

    #region Abstract
    protected abstract void CopyFields(TEntity target , TEntity source);
    #endregion Abstract
}