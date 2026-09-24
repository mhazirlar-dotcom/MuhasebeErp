using Accounting.Core.DataAccess.Persistence.Constants;
using Accounting.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data;

namespace Accounting.Core.DataAccess.Persistence.Master.Configurations;

public sealed class PermissionConfiguration : AuditableEntityConfiguration<Permission>
{
    #region Methods
    protected override void ConfigureEntity(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable(TableNames.Permissions);

        builder.Property(x => x.Code)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(128))
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(128))
            .IsRequired();

        builder.Property(x => x.Module)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(64))
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnType(SqlDbType.Bit.GetSqlType())
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("IX_Permissions_Code");

        builder.HasIndex(x => x.Module)
            .HasDatabaseName("IX_Permissions_Module");
    }
    #endregion Methods
}