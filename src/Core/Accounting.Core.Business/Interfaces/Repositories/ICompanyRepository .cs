using Accounting.Core.Domain.Entities;
using Accounting.Shared.Results;

namespace Accounting.Core.Business.Interfaces.Repositories;

public interface ICompanyRepository : IRepository<Company>
{
    #region Operations
    Task<Result<IReadOnlyList<Company>>> GetByUserIdAsync(
        Guid userId ,
        bool includeActive = true ,
        bool includePassive = false ,
        CancellationToken cancellationToken = default);

    Task<Result<Company>> AddWithUserAsync(
        Company company ,
        Guid userId ,
        bool isDefault ,
        CancellationToken cancellationToken = default);
    #endregion Operations
}