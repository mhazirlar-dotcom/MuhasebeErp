using Accounting.Shared.Results;
using FluentValidation;
using FluentValidation.Results;

namespace Accounting.Core.Business.Extensions;

public static class ValidationExtensions
{
    #region Operations
    public static async Task<IReadOnlyList<Error>?> ValidateErrorsAsync<TRequest>(this TRequest request , IValidator<TRequest> validator , CancellationToken cancellationToken = default)
    {
        ValidationResult result = await validator.ValidateAsync(request , cancellationToken);

        if (result.IsValid)
        {
            return null;
        }

        return [.. result.Errors.Select(failure => new Error(failure.ErrorCode , failure.ErrorMessage , ResultStatus.ValidationError , failure.PropertyName))];
    }

    public static async Task<Result<TRequest>> ValidateAsync<TRequest>(this TRequest request , IValidator<TRequest> validator , CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Error>? errors = await request.ValidateErrorsAsync(validator , cancellationToken);

        if (errors is null)
        {
            return Result<TRequest>.Success(request);
        }

        return Result<TRequest>.ValidationFailure(errors);
    }
    #endregion Operations
}