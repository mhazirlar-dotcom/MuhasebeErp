using System.Text.Json.Serialization;

namespace Accounting.Shared.Results;

public class Result : IResult
{
    #region Constructor
    [JsonConstructor]
    protected Result(string status , string message , IReadOnlyList<Error> errors)
    {
        Status = status;
        Message = message;
        Errors = errors;
    }
    #endregion Constructor

    #region Properties
    public bool IsSuccess => Status == ResultStatus.Success;
    public bool IsFailure => !IsSuccess;
    public string Status { get; }
    public string Message { get; }
    public IReadOnlyList<Error> Errors { get; }
    #endregion Properties

    #region Operations
    public static Result Success()
    {
        return new Result(ResultStatus.Success , string.Empty , []);
    }

    public static Result Success(string message)
    {
        return new Result(ResultStatus.Success , message , []);
    }

    public static Result Failure(string message , string status = ResultStatus.InternalError)
    {
        return new Result(status , message , [new Error(status , message , status , string.Empty)]);
    }

    public static Result ValidationFailure(IEnumerable<Error> errors)
    {
        IReadOnlyList<Error> errorList = [.. errors];
        string firstMessage = errorList.Count > 0 ? errorList[0].Message : string.Empty;

        return new Result(ResultStatus.ValidationError , firstMessage , errorList);
    }

    public static Result NotFound(string message)
    {
        return Failure(message , ResultStatus.NotFound);
    }

    public static Result Conflict(string message)
    {
        return Failure(message , ResultStatus.Conflict);
    }

    public static Result Unauthorized(string message)
    {
        return Failure(message , ResultStatus.Unauthorized);
    }

    public static Result Forbidden(string message)
    {
        return Failure(message , ResultStatus.Forbidden);
    }

    public static Result BusinessRuleViolation(string message)
    {
        return Failure(message , ResultStatus.BusinessRuleViolation);
    }
    #endregion Operations
}