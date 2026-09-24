using Accounting.Api.Controllers.Common;
using Accounting.Core.Business.Interfaces.Services;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Results;
using Microsoft.AspNetCore.Mvc;

namespace Accounting.Api.Controllers.Master;

[Route("api/periods")]
public sealed class PeriodsController(IPeriodService periodService) : CrudControllerBase<Period , IPeriodService>(periodService)
{
    #region Operations
    [HttpGet]
    public async Task<Result<IReadOnlyList<Period>>> GetByCompanyIdAsync([FromQuery] Guid companyId , CancellationToken cancellationToken)
    {
        return await _service.GetByCompanyIdAsync(companyId , cancellationToken);
    }
    #endregion Operations
}