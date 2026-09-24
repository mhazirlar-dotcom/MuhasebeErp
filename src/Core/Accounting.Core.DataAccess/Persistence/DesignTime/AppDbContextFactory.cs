using Accounting.Core.DataAccess.Persistence.Companies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Accounting.Core.DataAccess.Persistence.DesignTime;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    #region Constants
    private const string DesignTimeCompanyConnection =
        "Server=.;Database=Accounting_DesignTime;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";
    #endregion Constants

    #region Operations
    public AppDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<AppDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlServer(DesignTimeCompanyConnection);

        return new AppDbContext(
            optionsBuilder.Options ,
            new DesignTimeClock() ,
            new DesignTimeCurrentUser());
    }
    #endregion Operations
}