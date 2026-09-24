using System.Text.Json.Serialization;

namespace Accounting.Shared.Results;

public class Result<T> : Result, IDataResult<T>
{
    #region Constructor
    [JsonConstructor]
    private Result(string status , T data , string message , IReadOnlyList<Error> errors)
        : base(status , message , errors)
    {
        Data = data;
    }
    #endregion Constructor

    #region Properties
    public T Data { get; }
    #endregion Properties

    #region Operations
    public static Result<T> Success(T data)
    {
        return new Result<T>(ResultStatus.Success , data , string.Empty , []);
    }

    public static Result<T> Success(T data , string message)
    {
        return new Result<T>(ResultStatus.Success , data , message , []);
    }

    public static new Result<T> Failure(string message , string status = ResultStatus.InternalError)
    {
        return new Result<T>(status , default! , message , [new Error(status , message , status , string.Empty)]);
    }

    public static new Result<T> ValidationFailure(IEnumerable<Error> errors)
    {
        IReadOnlyList<Error> errorList = [.. errors];
        string firstMessage = errorList.Count > 0 ? errorList[0].Message : string.Empty;

        return new Result<T>(ResultStatus.ValidationError , default! , firstMessage , errorList);
    }

    public static new Result<T> NotFound(string message)
    {
        return Failure(message , ResultStatus.NotFound);
    }

    public static new Result<T> Conflict(string message)
    {
        return Failure(message , ResultStatus.Conflict);
    }

    public static new Result<T> Unauthorized(string message)
    {
        return Failure(message , ResultStatus.Unauthorized);
    }

    public static new Result<T> Forbidden(string message)
    {
        return Failure(message , ResultStatus.Forbidden);
    }

    public static new Result<T> BusinessRuleViolation(string message)
    {
        return Failure(message , ResultStatus.BusinessRuleViolation);
    }
    #endregion Operations
}