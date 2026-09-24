using Accounting.Api.Interfaces;
using Accounting.Api.Options;
using Accounting.Core.Business.Interfaces;
using Accounting.Core.Business.Interfaces.Repositories;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Dtos.Auth;
using Accounting.Shared.Markers;
using Accounting.Shared.Results;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace Accounting.Api.Services;

public sealed class AuthTokenService(
    IJwtTokenGenerator jwtTokenGenerator ,
    IRefreshTokenRepository refreshTokenRepository ,
    IUserRepository userRepository ,
    IOptions<JwtSettings> options ,
    IClock clock) : IAuthTokenService, IScopedService
{
    #region Constants
    private const int RefreshTokenByteLength = 64;
    #endregion Constants

    #region Fields
    private readonly JwtSettings _settings = options.Value;
    #endregion Fields

    #region Operations
    public async Task<Result<AuthResponse>> IssueTokensAsync(
        User user ,
        string ipAddress ,
        CancellationToken cancellationToken = default)
    {
        (string accessToken , DateTime accessExpiresAt) = jwtTokenGenerator.GenerateAccessToken(user);

        string refreshTokenPlain = GenerateRefreshTokenPlain();
        string refreshTokenHash = HashToken(refreshTokenPlain);

        DateTime now = clock.UtcNow;
        DateTime refreshExpiresAt = now.AddDays(_settings.RefreshTokenDays);

        RefreshToken entity = new()
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = refreshExpiresAt,
            IsRevoked = false,
            CreatedByIp = ipAddress
        };

        Result<RefreshToken> addResult = await refreshTokenRepository.CreateAsync(entity, cancellationToken);
        if (addResult.IsFailure)
        {
            return Result<AuthResponse>.Failure(addResult.Message , addResult.Status);
        }

        AuthResponse response = new(
            user.Id,
            user.UserName,
            user.FullName,
            accessToken,
            accessExpiresAt,
            refreshTokenPlain,
            refreshExpiresAt);

        return Result<AuthResponse>.Success(response);
    }

    public async Task<Result<AuthResponse>> RefreshAsync(
        string refreshToken ,
        string ipAddress ,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result<AuthResponse>.Unauthorized("Refresh token zorunludur.");
        }

        string hash = HashToken(refreshToken);

        Result<RefreshToken> tokenResult = await refreshTokenRepository.GetByTokenHashAsync(hash , cancellationToken);
        if (tokenResult.IsFailure)
        {
            return Result<AuthResponse>.Unauthorized("Geçersiz refresh token.");
        }

        RefreshToken existing = tokenResult.Data;

        if (existing.IsRevoked)
        {
            return Result<AuthResponse>.Unauthorized("Refresh token iptal edilmiş.");
        }

        DateTime now = clock.UtcNow;

        if (existing.ExpiresAt <= now)
        {
            return Result<AuthResponse>.Unauthorized("Refresh token süresi dolmuş.");
        }

        Result<User> userResult = await userRepository.GetByIdAsync(existing.UserId , cancellationToken);
        if (userResult.IsFailure)
        {
            return Result<AuthResponse>.Unauthorized("Kullanıcı bulunamadı.");
        }

        User user = userResult.Data;

        if (!user.IsActive)
        {
            return Result<AuthResponse>.Forbidden("Kullanıcı hesabı aktif değil.");
        }

        (string accessToken , DateTime accessExpiresAt) = jwtTokenGenerator.GenerateAccessToken(user);

        string newRefreshPlain = GenerateRefreshTokenPlain();
        string newRefreshHash = HashToken(newRefreshPlain);
        DateTime refreshExpiresAt = now.AddDays(_settings.RefreshTokenDays);

        existing.IsRevoked = true;
        existing.RevokedAt = now;
        existing.ReplacedByTokenHash = newRefreshHash;

        RefreshToken newEntity = new()
        {
            UserId = user.Id,
            TokenHash = newRefreshHash,
            ExpiresAt = refreshExpiresAt,
            IsRevoked = false,
            CreatedByIp = ipAddress
        };

        Result<RefreshToken> addResult = await refreshTokenRepository.CreateAsync(newEntity, cancellationToken);
        if (addResult.IsFailure)
        {
            return Result<AuthResponse>.Failure(addResult.Message , addResult.Status);
        }

        AuthResponse response = new(
            user.Id,
            user.UserName,
            user.FullName,
            accessToken,
            accessExpiresAt,
            newRefreshPlain,
            refreshExpiresAt);

        return Result<AuthResponse>.Success(response);
    }

    public async Task<Result> RevokeAsync(
        string refreshToken ,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Result.Success();
        }

        string hash = HashToken(refreshToken);

        Result<RefreshToken> tokenResult = await refreshTokenRepository.GetByTokenHashAsync(hash , cancellationToken);
        if (tokenResult.IsFailure)
        {
            return Result.Success();
        }

        RefreshToken existing = tokenResult.Data;

        if (existing.IsRevoked)
        {
            return Result.Success();
        }

        existing.IsRevoked = true;
        existing.RevokedAt = clock.UtcNow;

        return await refreshTokenRepository.SaveChangesAsync(cancellationToken);
    }
    #endregion Operations

    #region Helpers
    private static string GenerateRefreshTokenPlain()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(RefreshTokenByteLength);
        return Base64UrlEncoder.Encode(bytes);
    }

    private static string HashToken(string plainToken)
    {
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(plainToken));
        return Convert.ToHexString(hashBytes);
    }
    #endregion Helpers
}