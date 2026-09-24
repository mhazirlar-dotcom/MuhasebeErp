using Accounting.Shared.Results;

namespace Accounting.Desktop.Interfaces;

public interface IAuthTokenRefresher
{
    #region Operations
    Task<Result<bool>> TryRefreshAsync(CancellationToken cancellationToken = default);
    #endregion Operations
}