using Accounting.Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
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

        if (result.IsFailure)
        {
            string errors = string.Join(" ;; " , result.Errors.Select(e => $"[{e.Status}] {e.Field}: {e.Message}"));

            Log.Warning(
                "[ResultActionFilter] {Method} {Path} → {Status} | Message: {Message} | Errors: {Errors}" ,
                context.HttpContext.Request.Method ,
                context.HttpContext.Request.Path.Value ,
                result.Status ,
                result.Message ,
                errors);
        }

        executedContext.Result = result.ToActionResult();
    }
    #endregion Operations
}