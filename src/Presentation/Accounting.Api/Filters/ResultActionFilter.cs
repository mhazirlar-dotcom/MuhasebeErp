using Accounting.Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using IResult = Accounting.Shared.Results.IResult;

namespace Accounting.Api.Filters;

public sealed class ResultActionFilter : IAsyncActionFilter
{
    #region Operations
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context ,
        ActionExecutionDelegate next)
    {
        ActionExecutedContext executedContext = await next();

        if (executedContext.Result is not ObjectResult objectResult)
        {
            return;
        }

        if (objectResult.Value is not IResult result)
        {
            return;
        }

        executedContext.Result = result.ToActionResult();
    }
    #endregion Operations
}