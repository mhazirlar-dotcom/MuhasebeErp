namespace Accounting.Shared.Results;

public interface IResult
{
    #region Properties
    bool IsSuccess { get; }
    bool IsFailure { get; }
    string Status { get; }
    string Message { get; }
    IReadOnlyList<Error> Errors { get; }
    #endregion Properties
}