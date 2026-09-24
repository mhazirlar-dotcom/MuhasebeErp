using Accounting.Shared.Results;

namespace Accounting.Core.Business.Interfaces.Services;

public interface IStartupSeeder
{
    #region Operations
    Task<Result> SeedAsync(CancellationToken cancellationToken = default);
    #endregion Operations
}