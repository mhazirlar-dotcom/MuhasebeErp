namespace Accounting.Api.Options;

public class JwtSettings
{
    #region Constants
    public const string SectionName = "Jwt";
    #endregion Constants

    #region Properties
    public string Issuer { get; set; } = "Accounting.Api";
    public string Audience { get; set; } = "Accounting.Desktop";
    public string SecretKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 30;
    #endregion Properties
}