namespace Accounting.Shared.Dtos.Auth;

public sealed record FirstSetupRequest(
    string UserName ,
    string FullName ,
    string Email ,
    string Password);