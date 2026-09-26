using Accounting.Core.DataAccess.Persistence.Master;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Accounting.Core.DataAccess.Persistence.DesignTime;

public class MasterDbContextFactory : IDesignTimeDbContextFactory<MasterDbContext>
{
    #region Constants
    private const string DesignTimeMasterConnection =
        "Server=DESKTOP-DFL2OBF;Database=Accounting_Master;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";
    #endregion Constants

    #region Operations
    public MasterDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<MasterDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlServer(DesignTimeMasterConnection);

        return new MasterDbContext(
            optionsBuilder.Options ,
            new DesignTimeClock() ,
            new DesignTimeCurrentUser());
    }
    #endregion Operations
}