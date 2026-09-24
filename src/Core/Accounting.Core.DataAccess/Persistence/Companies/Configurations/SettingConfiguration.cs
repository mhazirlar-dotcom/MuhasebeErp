using Accounting.Core.DataAccess.Persistence.Constants;
using Accounting.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data;

namespace Accounting.Core.DataAccess.Persistence.Companies.Configurations;

public sealed class SettingConfiguration : AuditableEntityConfiguration<Setting>
{
    #region Methods
    protected override void ConfigureEntity(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable(TableNames.Settings);

        builder.Property(x => x.Key)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(128))
            .IsRequired();

        builder.Property(x => x.Value)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(2048))
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(512));

        builder.HasIndex(x => x.Key)
            .IsUnique()
            .HasDatabaseName("IX_Settings_Key");
    }
    #endregion Methods
}