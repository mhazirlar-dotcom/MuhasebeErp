using Accounting.Shared.Results;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Accounting.Desktop.Extensions;

public static class HttpResponseExtensions
{
    #region Constants
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    #endregion Constants

    #region Operations
    public static async Task<Result<T>> ReadResultAsync<T>(this HttpResponseMessage response , CancellationToken cancellationToken = default)
    {
        try
        {
            Result<T>? result = await response.Content.ReadFromJsonAsync<Result<T>>(JsonOptions , cancellationToken);

            if (result is null)
            {
                return Result<T>.Failure(BuildUnexpectedMessage(response) , ResultStatus.InternalError);
            }

            return result;
        }
        catch (JsonException)
        {
            return Result<T>.Failure(BuildUnexpectedMessage(response) , ResultStatus.InternalError);
        }
    }

    public static async Task<Result> ReadResultAsync(this HttpResponseMessage response , CancellationToken cancellationToken = default)
    {
        try
        {
            Result? result = await response.Content.ReadFromJsonAsync<Result>(JsonOptions , cancellationToken);

            if (result is null)
            {
                return Result.Failure(BuildUnexpectedMessage(response) , ResultStatus.InternalError);
            }

            return result;
        }
        catch (JsonException)
        {
            return Result.Failure(BuildUnexpectedMessage(response) , ResultStatus.InternalError);
        }
    }
    #endregion Operations

    #region Helpers
    private static string BuildUnexpectedMessage(HttpResponseMessage response)
    {
        return $"Sunucudan beklenmeyen yanıt: HTTP {(int)response.StatusCode} ({response.StatusCode}).";
    }
    #endregion Helpers
}