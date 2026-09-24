namespace Accounting.Shared.Dtos.Auth;

public sealed record AuthResponse(
    Guid UserId ,
    string UserName ,
    string FullName ,
    string AccessToken ,
    DateTime AccessTokenExpiresAt ,
    string RefreshToken ,
    DateTime RefreshTokenExpiresAt);