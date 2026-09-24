using Accounting.Core.Domain.Entities;
using Accounting.Shared.Dtos.Auth;
using Accounting.Shared.Results;

namespace Accounting.Core.Business.Interfaces.Services;

public interface IAuthService
{
    #region Operations
    Task<Result<bool>> HasAnyUserAsync(CancellationToken cancellationToken = default);

    Task<Result<User>> LoginAsync(LoginRequest request , string ipAddress , CancellationToken cancellationToken = default);

    Task<Result<User>> CreateFirstAdminAsync(FirstSetupRequest request , CancellationToken cancellationToken = default);
    #endregion Operations
}