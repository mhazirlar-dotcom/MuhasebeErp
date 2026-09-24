using Accounting.Api.Extensions;
using Accounting.Core.Business.Interfaces;
using Accounting.Shared.Markers;

namespace Accounting.Api.Services;

public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser, IScopedService
{
    #region Properties
    public Guid UserId => httpContextAccessor.GetUserId();

    public string UserName => httpContextAccessor.GetUserName();

    public bool IsAuthenticated => UserId != Guid.Empty;

    public string IpAddress => httpContextAccessor.GetRemoteIpAddress();
    #endregion Properties

    #region Operations
    public void SetUser(Guid userId , string userName , string ipAddress)
    {
        // JWT zaten kullanıcıyı taşır; bu metot API'de no-op.
    }

    public void Clear()
    {
        // JWT zaten kullanıcıyı taşır; bu metot API'de no-op.
    }
    #endregion Operations
}