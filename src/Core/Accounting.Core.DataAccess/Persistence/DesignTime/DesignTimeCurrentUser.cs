using Accounting.Core.Business.Interfaces;

namespace Accounting.Core.DataAccess.Persistence.DesignTime;

internal sealed class DesignTimeCurrentUser : ICurrentUser
{
    #region Properties
    public Guid UserId => Guid.Empty;
    public string UserName => "DesignTime";
    public bool IsAuthenticated => false;
    public string IpAddress => string.Empty;
    #endregion Properties

    #region Operations
    public void SetUser(Guid userId , string userName , string ipAddress)
    {

    }

    public void Clear()
    {

    }
    #endregion Operations
}