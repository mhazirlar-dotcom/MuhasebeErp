using Accounting.Desktop.Interfaces;
using Accounting.Shared.Dtos.Auth;
using Accounting.Shared.Markers;
using Serilog;

namespace Accounting.Desktop.Services;

public sealed class InMemoryTokenStore : ITokenStore, ILocalSingletonService
{
    #region Events
    public event EventHandler LoggedOut = delegate { };
    #endregion Events

    #region Fields
    private AuthResponse? _current;
    #endregion Fields

    #region Properties
    public AuthResponse? Current => _current;

    public bool HasToken => _current is not null && !string.IsNullOrEmpty(_current.AccessToken);
    #endregion Properties

    #region Operations
    public void Set(AuthResponse response)
    {
        _current = response;

        Log.Information("[InMemoryTokenStore] Set — StoreHash={StoreHash}, UserName={UserName}, HasToken={HasToken}" , GetHashCode() , response.UserName , HasToken);
    }

    public void Clear()
    {
        bool hadToken = _current is not null;

        _current = null;

        Log.Information("[InMemoryTokenStore] Clear — StoreHash={StoreHash}, HadToken={HadToken}" , GetHashCode() , hadToken);

        if (hadToken)
        {
            Log.Information("[InMemoryTokenStore] LoggedOut event tetikleniyor.");
            LoggedOut.Invoke(this , EventArgs.Empty);
        }
    }
    #endregion Operations
}