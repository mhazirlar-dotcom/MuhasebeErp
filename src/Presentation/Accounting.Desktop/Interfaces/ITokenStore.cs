using Accounting.Shared.Dtos.Auth;

namespace Accounting.Desktop.Interfaces;

public interface ITokenStore
{
    #region Events
    event EventHandler LoggedOut;
    #endregion Events

    #region Properties
    AuthResponse? Current { get; }
    bool HasToken { get; }
    #endregion Properties

    #region Operations
    void Set(AuthResponse response);
    void Clear();
    #endregion Operations
}