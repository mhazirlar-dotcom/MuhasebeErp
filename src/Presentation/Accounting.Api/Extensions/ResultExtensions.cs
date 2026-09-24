using Accounting.Shared.Results;
using Microsoft.AspNetCore.Mvc;
using IResult = Accounting.Shared.Results.IResult;

namespace Accounting.Api.Extensions;

public static class ResultExtensions
{
    #region Operations
    public static IActionResult ToActionResult(this IResult result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result);
        }

        return BuildErrorResult(result.Status , result);
    }
    #endregion Operations

    #region Helpers
    private static IActionResult BuildErrorResult(string status , IResult result)
    {
        int statusCode = status switch
        {
            ResultStatus.ValidationError => StatusCodes.Status400BadRequest ,
            ResultStatus.Unauthorized => StatusCodes.Status401Unauthorized ,
            ResultStatus.Forbidden => StatusCodes.Status403Forbidden ,
            ResultStatus.NotFound => StatusCodes.Status404NotFound ,
            ResultStatus.Conflict => StatusCodes.Status409Conflict ,
            ResultStatus.BusinessRuleViolation => StatusCodes.Status422UnprocessableEntity ,
            ResultStatus.InternalError => StatusCodes.Status500InternalServerError ,
            _ => StatusCodes.Status500InternalServerError
        };

        return new ObjectResult(result)
        {
            StatusCode = statusCode
        };
    }
    #endregion Helpers
}