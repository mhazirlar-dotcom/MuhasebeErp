namespace Accounting.Shared.Results;

public static class ResultExtensions
{
    #region Generic Result<T>
    public static async Task<Result<T>> MapFailureAsync<T>(this Task<Result<T>> task , string message , string status)
    {
        Result<T> result = await task;

        if (result.IsSuccess)
        {
            return result;
        }

        return Result<T>.Failure(message , status);
    }

    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> task , Func<T , bool> predicate , string message , string status)
    {
        Result<T> result = await task;

        if (result.IsFailure)
        {
            return result;
        }

        if (predicate(result.Data))
        {
            return result;
        }

        return Result<T>.Failure(message , status);
    }

    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Task<Result<TIn>> task , Func<TIn , Task<Result<TOut>>> next)
    {
        Result<TIn> result = await task;

        if (result.IsFailure)
        {
            return Result<TOut>.Failure(result.Message , result.Status);
        }

        return await next(result.Data);
    }
    #endregion Generic Result<T>

    #region Bool Result
    public static async Task<Result> EnsureFalseAsync(this Task<Result<bool>> task , string message , string status)
    {
        Result<bool> result = await task;

        if (result.IsFailure)
        {
            return Result.Failure(result.Message , result.Status);
        }

        if (result.Data)
        {
            return Result.Failure(message , status);
        }

        return Result.Success();
    }
    #endregion Bool Result

    #region Non-Generic Result
    public static async Task<Result<TOut>> ThenAsync<TOut>(this Task<Result> task , Func<Task<Result<TOut>>> next)
    {
        Result result = await task;

        if (result.IsFailure)
        {
            return Result<TOut>.Failure(result.Message , result.Status);
        }

        return await next();
    }

    public static async Task<Result> ThenAsync(this Task<Result> task , Func<Task<Result>> next)
    {
        Result result = await task;

        if (result.IsFailure)
        {
            return result;
        }

        return await next();
    }
    #endregion Non-Generic Result
}