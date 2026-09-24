namespace Accounting.Core.DataAccess.Persistence.Constants;

public static class TableNames
{
    #region Master
    public const string Companies = nameof(Companies);
    public const string Users = nameof(Users);
    public const string UserCompanies = nameof(UserCompanies);
    public const string Roles = nameof(Roles);
    public const string UserRoles = nameof(UserRoles);
    public const string Permissions = nameof(Permissions);
    public const string RolePermissions = nameof(RolePermissions);
    public const string LookupValues = nameof(LookupValues);
    public const string Periods = nameof(Periods);
    public const string RefreshTokens = nameof(RefreshTokens);
    #endregion Master

    #region Company
    public const string Settings = nameof(Settings);
    public const string NumberSequences = nameof(NumberSequences);
    public const string AuditLogs = nameof(AuditLogs);
    #endregion Company
}