namespace Accounting.Core.Business.Interfaces;

public interface ICurrentUser
{
    #region Properties
    Guid UserId { get; }
    string UserName { get; }
    bool IsAuthenticated { get; }
    string IpAddress { get; }
    #endregion Properties

    #region Operations
    void SetUser(Guid userId , string userName , string ipAddress);
    void Clear();
    #endregion Operations
}