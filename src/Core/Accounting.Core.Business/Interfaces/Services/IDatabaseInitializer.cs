using Accounting.Core.Domain.Entities;
using Accounting.Shared.Results;

namespace Accounting.Core.Business.Interfaces.Services;

public interface IDatabaseInitializer
{
    #region Operations
    Task<Result> InitializeCompanyDatabaseAsync(Company company , CancellationToken cancellationToken = default);
    #endregion Operations
}