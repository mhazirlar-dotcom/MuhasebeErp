using Accounting.Shared.Results;

namespace Accounting.Core.Business.Interfaces.Services;

public interface ICompanyDatabaseMigrator
{
    #region Operations
    Task<Result<int>> MigrateAllAsync(CancellationToken cancellationToken = default);

    Task<Result> MigrateOneAsync(Guid companyId , CancellationToken cancellationToken = default);
    #endregion Operations
}