namespace Accounting.Shared.Results;

public static class ResultStatus
{
    #region Constants
    public const string Success = "success";
    public const string ValidationError = "validation_error";
    public const string Unauthorized = "unauthorized";
    public const string Forbidden = "forbidden";
    public const string NotFound = "not_found";
    public const string Conflict = "conflict";
    public const string BusinessRuleViolation = "business_rule_violation";
    public const string InternalError = "internal_error";
    #endregion Constants
}