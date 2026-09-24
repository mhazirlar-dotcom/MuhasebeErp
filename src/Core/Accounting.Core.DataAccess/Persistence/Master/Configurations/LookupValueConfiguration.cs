using Accounting.Core.DataAccess.Persistence.Constants;
using Accounting.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data;

namespace Accounting.Core.DataAccess.Persistence.Master.Configurations;

public sealed class LookupValueConfiguration : AuditableEntityConfiguration<LookupValue>
{
    #region Methods
    protected override void ConfigureEntity(EntityTypeBuilder<LookupValue> builder)
    {
        builder.ToTable(TableNames.LookupValues);

        builder.Property(x => x.Type)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(64))
            .IsRequired();

        builder.Property(x => x.Code)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(64))
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType())
            .IsRequired();

        builder.Property(x => x.ParentId)
            .HasColumnType(SqlDbType.UniqueIdentifier.GetSqlType());

        builder.Property(x => x.DisplayOrder)
            .HasColumnType(SqlDbType.Int.GetSqlType())
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnType(SqlDbType.Bit.GetSqlType())
            .IsRequired();

        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.Type , x.Code })
            .IsUnique()
            .HasDatabaseName("IX_LookupValues_Type_Code");

        builder.HasIndex(x => new { x.Type , x.DisplayOrder })
            .HasDatabaseName("IX_LookupValues_Type_DisplayOrder");

        builder.HasIndex(x => x.ParentId)
            .HasDatabaseName("IX_LookupValues_ParentId");
    }
    #endregion Methods
}