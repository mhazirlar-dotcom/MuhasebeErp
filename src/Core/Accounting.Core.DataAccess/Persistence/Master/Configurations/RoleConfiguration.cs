using Accounting.Core.DataAccess.Persistence.Constants;
using Accounting.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data;

namespace Accounting.Core.DataAccess.Persistence.Master.Configurations;

public sealed class RoleConfiguration : AuditableEntityConfiguration<Role>
{
    #region Methods
    protected override void ConfigureEntity(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable(TableNames.Roles);

        builder.Property(x => x.Name)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(64))
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(512));

        builder.Property(x => x.IsActive)
            .HasColumnType(SqlDbType.Bit.GetSqlType())
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique()
            .HasDatabaseName("IX_Roles_Name");
    }
    #endregion Methods
}