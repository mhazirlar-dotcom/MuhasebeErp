using Accounting.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Accounting.Api.Controllers.Common;

[ApiController]
[Authorize]
public abstract class ApiControllerBase : ControllerBase
{
    #region Helpers
    protected string ResolveIpAddress()
    {
        return HttpContext.ResolveClientIpAddress();
    }
    #endregion Helpers
}