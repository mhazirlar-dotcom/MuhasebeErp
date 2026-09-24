using Accounting.Core.Business.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Core.DataAccess.Persistence.Companies;

public class AppDbContext(
    DbContextOptions<AppDbContext> options ,
    IClock clock ,
    ICurrentUser currentUser) : BaseDbContext(options , clock , currentUser)
{
    #region Properties
    protected override bool SupportsAuditLog => true;
    #endregion Properties

    #region Configuration
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly ,
            type => type.Namespace != null
                 && type.Namespace.EndsWith("Persistence.Companies.Configurations"));

        base.OnModelCreating(modelBuilder);
    }
    #endregion Configuration
}