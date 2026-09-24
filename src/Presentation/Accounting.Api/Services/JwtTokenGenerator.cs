using Accounting.Api.Interfaces;
using Accounting.Api.Options;
using Accounting.Core.Business.Interfaces;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Markers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Accounting.Api.Services;

public sealed class JwtTokenGenerator(
    IOptions<JwtSettings> options ,
    IClock clock) : IJwtTokenGenerator, ISingletonService
{
    #region Fields
    private readonly JwtSettings _settings = options.Value;
    #endregion Fields

    #region Operations
    public (string Token , DateTime ExpiresAt) GenerateAccessToken(User user)
    {
        DateTime issuedAt = clock.UtcNow;
        DateTime expiresAt = issuedAt.AddMinutes(_settings.AccessTokenMinutes);

        SymmetricSecurityKey securityKey = new(
            Encoding.UTF8.GetBytes(_settings.SecretKey));

        SigningCredentials credentials = new(securityKey , SecurityAlgorithms.HmacSha256);

        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub , user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName , user.UserName),
            new(JwtRegisteredClaimNames.Name , user.FullName),
            new(JwtRegisteredClaimNames.Email , user.Email),
            new(JwtRegisteredClaimNames.Jti , Guid.NewGuid().ToString("N"))
        ];

        SecurityTokenDescriptor descriptor = new()
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            IssuedAt = issuedAt,
            NotBefore = issuedAt,
            Expires = expiresAt,
            SigningCredentials = credentials
        };

        JsonWebTokenHandler handler = new();
        string token = handler.CreateToken(descriptor);

        return (token , expiresAt);
    }
    #endregion Operations
}