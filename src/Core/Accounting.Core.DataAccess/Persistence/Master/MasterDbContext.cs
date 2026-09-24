using Accounting.Core.Business.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Core.DataAccess.Persistence.Master;

public class MasterDbContext(
    DbContextOptions<MasterDbContext> options ,
    IClock clock ,
    ICurrentUser currentUser) : BaseDbContext(options , clock , currentUser)
{
    #region Properties
    protected override bool SupportsAuditLog => false;
    #endregion Properties

    #region Configuration
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(MasterDbContext).Assembly ,
            type => type.Namespace != null
                 && type.Namespace.EndsWith("Persistence.Master.Configurations"));

        base.OnModelCreating(modelBuilder);
    }
    #endregion Configuration
}