namespace Accounting.Shared.Dtos.Auth;

public sealed record LoginRequest(
    string UserName ,
    string Password);