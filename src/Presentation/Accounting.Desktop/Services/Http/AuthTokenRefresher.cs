using Accounting.Desktop.Extensions;
using Accounting.Desktop.Interfaces;
using Accounting.Shared.Dtos.Auth;
using Accounting.Shared.Markers;
using Accounting.Shared.Results;
using Serilog;
using System.Net.Http;
using System.Net.Http.Json;

namespace Accounting.Desktop.Services.Http;

public sealed class AuthTokenRefresher(IHttpClientFactory httpClientFactory , ITokenStore tokenStore) : IAuthTokenRefresher, ILocalSingletonService
{
    #region Constants
    private const string RefreshClientName = "AuthRefreshClient";
    #endregion Constants

    #region Operations
    public async Task<Result<bool>> TryRefreshAsync(CancellationToken cancellationToken = default)
    {
        if (tokenStore.Current is null)
        {
            Log.Warning("[AuthTokenRefresher] Refresh denendi ama token store boş.");
            return Result<bool>.Failure("Refresh token bulunamadı." , ResultStatus.Unauthorized);
        }

        string refreshToken = tokenStore.Current.RefreshToken;

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            Log.Warning("[AuthTokenRefresher] Refresh denendi ama refresh token boş.");
            return Result<bool>.Failure("Refresh token bulunamadı." , ResultStatus.Unauthorized);
        }

        RefreshTokenRequest request = new(refreshToken);

        HttpClient httpClient = httpClientFactory.CreateClient(RefreshClientName);

        Log.Information("[AuthTokenRefresher] Refresh isteği gönderiliyor — Uri={Uri}" , httpClient.BaseAddress);

        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/auth/refresh" , request , cancellationToken);
        Result<AuthResponse> refreshResult = await response.ReadResultAsync<AuthResponse>(cancellationToken);

        if (refreshResult.IsFailure)
        {
            Log.Warning("[AuthTokenRefresher] Refresh başarısız — Status={Status}, Message={Message}" , refreshResult.Status , refreshResult.Message);
            return Result<bool>.Failure(refreshResult.Message , refreshResult.Status);
        }

        tokenStore.Set(refreshResult.Data);

        Log.Information("[AuthTokenRefresher] Refresh başarılı — UserName={UserName}" , refreshResult.Data.UserName);

        return Result<bool>.Success(true , "Token yenilendi.");
    }
    #endregion Operations
}