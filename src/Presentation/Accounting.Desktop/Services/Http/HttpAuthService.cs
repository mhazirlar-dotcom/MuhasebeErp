using Accounting.Core.Business.Interfaces.Services;
using Accounting.Core.Domain.Entities;
using Accounting.Desktop.Extensions;
using Accounting.Desktop.Interfaces;
using Accounting.Shared.Dtos.Auth;
using Accounting.Shared.Markers;
using Accounting.Shared.Results;
using System.Net.Http;
using System.Net.Http.Json;

namespace Accounting.Desktop.Services.Http;

public sealed class HttpAuthService(IHttpClientFactory httpClientFactory , ITokenStore tokenStore) : IAuthService, IApiClient
{
    #region Fields
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
    #endregion Fields

    #region Operations
    public async Task<Result<bool>> HasAnyUserAsync(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.GetAsync("api/auth/has-user" , cancellationToken);

        return await response.ReadResultAsync<bool>(cancellationToken);
    }

    public async Task<Result<User>> LoginAsync(LoginRequest request , string ipAddress , CancellationToken cancellationToken = default)
    {
        _ = ipAddress;   // IP adresi API tarafında RemoteIpAddress'ten alınır.

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/auth/login" , request , cancellationToken);
        Result<AuthResponse> authResult = await response.ReadResultAsync<AuthResponse>(cancellationToken);

        if (authResult.IsFailure)
        {
            return Result<User>.Failure(authResult.Message , authResult.Status);
        }

        tokenStore.Set(authResult.Data);

        return Result<User>.Success(BuildUserFromAuthResponse(authResult.Data) , "Giriş başarılı.");
    }

    public async Task<Result<User>> CreateFirstAdminAsync(FirstSetupRequest request , CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/auth/first-setup" , request , cancellationToken);
        Result<AuthResponse> authResult = await response.ReadResultAsync<AuthResponse>(cancellationToken);

        if (authResult.IsFailure)
        {
            return Result<User>.Failure(authResult.Message , authResult.Status);
        }

        tokenStore.Set(authResult.Data);

        return Result<User>.Success(BuildUserFromAuthResponse(authResult.Data) , "Yönetici hesabı oluşturuldu.");
    }
    #endregion Operations

    #region Helpers
    private static User BuildUserFromAuthResponse(AuthResponse response)
    {
        return new User
        {
            Id = response.UserId ,
            UserName = response.UserName ,
            FullName = response.FullName
        };
    }
    #endregion Helpers
}