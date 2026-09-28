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
using Serilog;

namespace Accounting.Core.DataAccess.Services;

public class CompanyDatabaseInitializer(IOptions<CompanyDatabaseOptions> options , IServiceProvider serviceProvider) : IDatabaseInitializer, IScopedService
{
    #region Constants
    private const string MigrationsAssembly = "Accounting.Core.DataAccess";
    #endregion Constants

    #region Operations
    public async Task<Result> InitializeCompanyDatabaseAsync(Company company , CancellationToken cancellationToken = default)
    {
        try
        {
            string connectionString = CompanyConnectionStringBuilder.BuildFromOptions(options.Value , company);

            DbContextOptionsBuilder<AppDbContext> optionsBuilder = new();
            optionsBuilder.UseSqlServer(
                connectionString ,
                sqlOptions => sqlOptions.MigrationsAssembly(MigrationsAssembly));

            IClock clock = serviceProvider.GetRequiredService<IClock>();
            ICurrentUser currentUser = serviceProvider.GetRequiredService<ICurrentUser>();

            AppDbContext dbContext = new(optionsBuilder.Options , clock , currentUser);

            IEnumerable<string> pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);

            if (!pendingMigrations.Any())
            {
                Log.Debug(
                    "[CompanyDatabaseInitializer] Bekleyen migration yok, atlanıyor — CompanyId={CompanyId}, Database={Database}" ,
                    company.Id , company.DatabaseName);

                return Result.Success("Firma veritabanı güncel.");
            }

            Log.Information(
                "[CompanyDatabaseInitializer] MigrateAsync başlıyor — CompanyId={CompanyId}, Database={Database}, PendingCount={PendingCount}, MigrationsAssembly={Assembly}" ,
                company.Id , company.DatabaseName , pendingMigrations.Count() , MigrationsAssembly);

            await dbContext.Database.MigrateAsync(cancellationToken);

            IEnumerable<string> appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync(cancellationToken);

            Log.Information(
                "[CompanyDatabaseInitializer] MigrateAsync tamamlandı — CompanyId={CompanyId}, AppliedCount={AppliedCount}" ,
                company.Id , appliedMigrations.Count());

            return Result.Success("Firma veritabanı hazır.");
        }
        catch (Exception ex)
        {
            Log.Error(ex ,
                "[CompanyDatabaseInitializer] MigrateAsync hata — CompanyId={CompanyId}, Database={Database}" ,
                company.Id , company.DatabaseName);

            return Result.Failure($"Veritabanı kurulumu başarısız: {ex.Message}" , ResultStatus.InternalError);
        }
    }
    #endregion Operations
}