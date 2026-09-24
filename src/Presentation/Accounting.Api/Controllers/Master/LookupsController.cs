using Accounting.Api.Controllers.Common;
using Accounting.Core.Business.Interfaces.Services;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Results;
using Microsoft.AspNetCore.Mvc;

namespace Accounting.Api.Controllers.Master;

[Route("api/lookups")]
public sealed class LookupsController(ILookupValueService lookupValueService) : ApiControllerBase
{
    #region Operations
    [HttpGet("{type}")]
    public async Task<Result<IReadOnlyList<LookupValue>>> GetByTypeAsync(
        string type ,
        [FromQuery] bool includeInactive = false ,
        CancellationToken cancellationToken = default)
    {
        return await lookupValueService.GetByTypeAsync(
            type ,
            includeInactive ,
            cancellationToken);
    }

    [HttpGet("{type}/{code}")]
    public async Task<Result<LookupValue>> GetByTypeAndCodeAsync(
        string type ,
        string code ,
        CancellationToken cancellationToken)
    {
        return await lookupValueService.GetByTypeAndCodeAsync(
            type ,
            code ,
            cancellationToken);
    }

    [HttpGet("children/{parentId:guid}")]
    public async Task<Result<IReadOnlyList<LookupValue>>> GetChildrenAsync(
        Guid parentId ,
        [FromQuery] bool includeInactive = false ,
        CancellationToken cancellationToken = default)
    {
        return await lookupValueService.GetChildrenAsync(
            parentId ,
            includeInactive ,
            cancellationToken);
    }
    #endregion Operations
}