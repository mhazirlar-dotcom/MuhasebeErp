using System.Security.Claims;

namespace Accounting.Api.Extensions;

public static class HttpContextExtensions
{
    #region Constants
    private const string ClaimSub = "sub";
    private const string ClaimUniqueName = "unique_name";
    private const string HeaderCompanyId = "X-Company-Id";
    private const string HeaderPeriodId = "X-Period-Id";
    private const string UnknownIp = "unknown";
    #endregion Constants

    #region Operations
    public static Guid GetUserId(this IHttpContextAccessor httpContextAccessor)
    {
        string? value = GetClaimValue(httpContextAccessor , ClaimSub);
        return Guid.TryParse(value , out Guid userId) ? userId : Guid.Empty;
    }

    public static string GetUserName(this IHttpContextAccessor httpContextAccessor)
    {
        return GetClaimValue(httpContextAccessor , ClaimUniqueName) ?? string.Empty;
    }

    public static string GetRemoteIpAddress(this IHttpContextAccessor httpContextAccessor)
    {
        string? remoteIp = httpContextAccessor.HttpContext?
            .Connection.RemoteIpAddress?.ToString();

        return string.IsNullOrWhiteSpace(remoteIp) ? UnknownIp : remoteIp;
    }

    public static Guid GetCompanyId(this IHttpContextAccessor httpContextAccessor)
    {
        return ReadHeaderAsGuid(httpContextAccessor , HeaderCompanyId);
    }

    public static Guid GetPeriodId(this IHttpContextAccessor httpContextAccessor)
    {
        return ReadHeaderAsGuid(httpContextAccessor , HeaderPeriodId);
    }

    public static string ResolveClientIpAddress(this HttpContext httpContext)
    {
        string? remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();
        return string.IsNullOrWhiteSpace(remoteIp) ? UnknownIp : remoteIp;
    }
    #endregion Operations

    #region Helpers
    private static string? GetClaimValue(IHttpContextAccessor httpContextAccessor , string claimType)
    {
        ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
        return user?.FindFirst(claimType)?.Value;
    }

    private static Guid ReadHeaderAsGuid(IHttpContextAccessor httpContextAccessor , string headerName)
    {
        string? value = httpContextAccessor.HttpContext?
            .Request.Headers[headerName]
            .FirstOrDefault();

        return Guid.TryParse(value , out Guid id) ? id : Guid.Empty;
    }
    #endregion Helpers
}