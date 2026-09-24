using Accounting.Core.Domain.Entities;
using Accounting.Shared.Results;

namespace Accounting.Core.Business.Interfaces.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    #region Operations
    Task<Result<RefreshToken>> GetByTokenHashAsync(
        string tokenHash ,
        CancellationToken cancellationToken = default);
    #endregion Operations
}