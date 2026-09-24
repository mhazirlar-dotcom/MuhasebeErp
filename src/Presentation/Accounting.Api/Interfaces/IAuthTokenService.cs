using Accounting.Core.Domain.Entities;
using Accounting.Shared.Dtos.Auth;
using Accounting.Shared.Results;

namespace Accounting.Api.Interfaces;

public interface IAuthTokenService
{
    #region Operations
    Task<Result<AuthResponse>> IssueTokensAsync(
        User user ,
        string ipAddress ,
        CancellationToken cancellationToken = default);

    Task<Result<AuthResponse>> RefreshAsync(
        string refreshToken ,
        string ipAddress ,
        CancellationToken cancellationToken = default);

    Task<Result> RevokeAsync(
        string refreshToken ,
        CancellationToken cancellationToken = default);
    #endregion Operations
}