using Accounting.Api.Controllers.Common;
using Accounting.Api.Interfaces;
using Accounting.Core.Business.Interfaces.Services;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Dtos.Auth;
using Accounting.Shared.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Accounting.Api.Controllers.Master;

[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController(IAuthService authService , IAuthTokenService authTokenService) : ApiControllerBase
{
    #region Operations
    [HttpPost("login")]
    public async Task<Result<AuthResponse>> LoginAsync([FromBody] LoginRequest request , CancellationToken cancellationToken)
    {
        string ipAddress = ResolveIpAddress();

        Result<User> loginResult = await authService.LoginAsync(request , ipAddress , cancellationToken);

        if (loginResult.IsFailure)
        {
            return Result<AuthResponse>.Failure(loginResult.Message , loginResult.Status);
        }

        return await authTokenService.IssueTokensAsync(loginResult.Data , ipAddress , cancellationToken);
    }

    [HttpPost("first-setup")]
    public async Task<Result<AuthResponse>> FirstSetupAsync([FromBody] FirstSetupRequest request , CancellationToken cancellationToken)
    {
        string ipAddress = ResolveIpAddress();

        Result<User> setupResult = await authService.CreateFirstAdminAsync(request , cancellationToken);

        if (setupResult.IsFailure)
        {
            return Result<AuthResponse>.Failure(setupResult.Message , setupResult.Status);
        }

        return await authTokenService.IssueTokensAsync(setupResult.Data , ipAddress , cancellationToken);
    }

    [HttpPost("refresh")]
    public async Task<Result<AuthResponse>> RefreshAsync([FromBody] RefreshTokenRequest request , CancellationToken cancellationToken)
    {
        string ipAddress = ResolveIpAddress();

        return await authTokenService.RefreshAsync(request.RefreshToken , ipAddress , cancellationToken);
    }

    [HttpPost("revoke")]
    public async Task<Result> RevokeAsync([FromBody] RefreshTokenRequest request , CancellationToken cancellationToken)
    {
        return await authTokenService.RevokeAsync(request.RefreshToken , cancellationToken);
    }

    [HttpGet("has-user")]
    public async Task<Result<bool>> HasUserAsync(CancellationToken cancellationToken)
    {
        return await authService.HasAnyUserAsync(cancellationToken);
    }
    #endregion Operations
}