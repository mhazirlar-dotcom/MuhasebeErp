using Accounting.Core.DataAccess.Persistence.Constants;
using Accounting.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data;

namespace Accounting.Core.DataAccess.Persistence.Companies.Configurations;

public sealed class AuditLogConfiguration : BaseEntityConfiguration<AuditLog>
{
    #region Methods
    protected override void ConfigureEntity(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable(TableNames.AuditLogs);

        builder.Property(x => x.UserId)
            .HasColumnType(SqlDbType.UniqueIdentifier.GetSqlType())
            .IsRequired();

        builder.Property(x => x.UserName)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(128))
            .IsRequired();

        builder.Property(x => x.Action)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(32))
            .IsRequired();

        builder.Property(x => x.EntityName)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(128))
            .IsRequired();

        builder.Property(x => x.EntityId)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(64))
            .IsRequired();

        // AuditLog istisnası: OldValues/NewValues JSON olup kesilemez, nvarchar(max) kullanılır.
        builder.Property(x => x.OldValues)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType());

        // AuditLog istisnası: OldValues/NewValues JSON olup kesilemez, nvarchar(max) kullanılır.
        builder.Property(x => x.NewValues)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType());

        builder.Property(x => x.Timestamp)
            .HasColumnType(SqlDbType.DateTime2.GetSqlType())
            .IsRequired();

        builder.Property(x => x.IpAddress)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(45));

        builder.HasIndex(x => x.Timestamp)
            .HasDatabaseName("IX_AuditLogs_Timestamp");

        builder.HasIndex(x => new { x.EntityName , x.EntityId })
            .HasDatabaseName("IX_AuditLogs_EntityName_EntityId");

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_AuditLogs_UserId");
    }
    #endregion Methods
}