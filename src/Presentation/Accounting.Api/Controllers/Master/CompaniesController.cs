using Accounting.Api.Controllers.Common;
using Accounting.Core.Business.Interfaces.Services;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Dtos.Master;
using Accounting.Shared.Results;
using Microsoft.AspNetCore.Mvc;

namespace Accounting.Api.Controllers.Master;

[Route("api/companies")]
public sealed class CompaniesController(ICompanyService companyService) : CrudControllerBase<Company , ICompanyService>(companyService)
{
    #region Operations
    [HttpGet]
    public async Task<Result<IReadOnlyList<Company>>> GetAllAsync([FromQuery] bool includeActive = true , [FromQuery] bool includePassive = false , CancellationToken cancellationToken = default)
    {
        return await _service.GetByUserIdAsync(includeActive , includePassive , cancellationToken);
    }

    [HttpPut("{id:guid}/active")]
    public Task<Result> SetActiveAsync(Guid id , [FromBody] SetCompanyActiveRequest request , CancellationToken cancellationToken)
    {
        return _service.SetActiveAsync(id , request.IsActive , cancellationToken);
    }

    [HttpDelete("{id:guid}")]
    public override Task<Result> DeleteAsync(Guid id , CancellationToken cancellationToken)
    {
        Result result = Result.BusinessRuleViolation("Firma silinemez. Pasife almak için 'PUT /api/companies/{id}/active' endpoint'ini kullanın.");
        return Task.FromResult(result);
    }
    #endregion Operations
}