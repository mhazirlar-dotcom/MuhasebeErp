using Accounting.Core.Business.Interfaces;
using Accounting.Shared.Markers;

namespace Accounting.Core.Business.Services;

public sealed class CurrentUser : ICurrentUser, ILocalSingletonService
{
    #region Properties
    public Guid UserId { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public bool IsAuthenticated => UserId != Guid.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    #endregion Properties

    #region Operations
    public void SetUser(Guid userId , string userName , string ipAddress)
    {
        UserId = userId;
        UserName = userName;
        IpAddress = ipAddress;
    }

    public void Clear()
    {
        UserId = Guid.Empty;
        UserName = string.Empty;
        IpAddress = string.Empty;
    }
    #endregion Operations
}