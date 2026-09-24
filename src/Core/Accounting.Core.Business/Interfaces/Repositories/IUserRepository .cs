using Accounting.Core.Domain.Entities;
using Accounting.Shared.Results;

namespace Accounting.Core.Business.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    #region Operations
    Task<Result<User>> GetByUserNameAsync(string userName , CancellationToken cancellationToken = default);
    Task<Result<User>> GetByEmailAsync(string email , CancellationToken cancellationToken = default);
    #endregion Operations
}