using Accounting.Core.Business.Interfaces;
using Accounting.Core.Domain.Common;
using Accounting.Shared.Results;
using Microsoft.AspNetCore.Mvc;

namespace Accounting.Api.Controllers.Common;

public abstract class CrudControllerBase<TEntity, TService>(TService service) : ApiControllerBase
    where TEntity : BaseEntity
    where TService : ICrudOperations<TEntity>
{
    #region Fields
    protected readonly TService _service = service;
    #endregion Fields

    #region Operations
    [HttpGet("{id:guid}")]
    public virtual Task<Result<TEntity>> GetByIdAsync(Guid id , CancellationToken cancellationToken)
    {
        return _service.GetByIdAsync(id , cancellationToken);
    }

    [HttpPost]
    public virtual Task<Result<TEntity>> CreateAsync([FromBody] TEntity entity , CancellationToken cancellationToken)
    {
        //entity.Id = Guid.NewGuid();
        return _service.CreateAsync(entity , cancellationToken);
    }

    [HttpPut("{id:guid}")]
    public virtual Task<Result> UpdateAsync(Guid id , [FromBody] TEntity entity , CancellationToken cancellationToken)
    {
        entity.Id = id;
        return _service.UpdateAsync(entity , cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    public virtual Task<Result> DeleteAsync(Guid id , CancellationToken cancellationToken)
    {
        return _service.DeleteAsync(id , cancellationToken);
    }
    #endregion Operations
}