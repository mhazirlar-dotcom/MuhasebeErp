using Accounting.Core.Business.Interfaces;
using Accounting.Core.Business.Interfaces.Services;
using Accounting.Core.Business.Options;
using Accounting.Core.DataAccess.Persistence;
using Accounting.Core.DataAccess.Persistence.Companies;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Markers;
using Accounting.Shared.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Accounting.Core.DataAccess.Services;

public class CompanyDatabaseInitializer(IOptions<CompanyDatabaseOptions> options , IServiceProvider serviceProvider) : IDatabaseInitializer, IScopedService
{
    #region Operations
    public async Task<Result> InitializeCompanyDatabaseAsync(Company company , CancellationToken cancellationToken = default)
    {
        try
        {
            string connectionString = CompanyConnectionStringBuilder.BuildFromOptions(options.Value , company);

            DbContextOptionsBuilder<AppDbContext> optionsBuilder = new();
            optionsBuilder.UseSqlServer(connectionString);

            IClock clock = serviceProvider.GetRequiredService<IClock>();
            ICurrentUser currentUser = serviceProvider.GetRequiredService<ICurrentUser>();

            AppDbContext dbContext = new(optionsBuilder.Options , clock , currentUser);

            await dbContext.Database.MigrateAsync(cancellationToken);

            return Result.Success("Firma veritabanı hazır.");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Veritabanı kurulumu başarısız: {ex.Message}" , ResultStatus.InternalError);
        }
    }
    #endregion Operations
}