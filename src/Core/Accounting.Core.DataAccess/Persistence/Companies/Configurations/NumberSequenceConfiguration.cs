using Accounting.Core.DataAccess.Persistence.Constants;
using Accounting.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data;

namespace Accounting.Core.DataAccess.Persistence.Companies.Configurations;

public sealed class NumberSequenceConfiguration : AuditableEntityConfiguration<NumberSequence>
{
    #region Methods
    protected override void ConfigureEntity(EntityTypeBuilder<NumberSequence> builder)
    {
        builder.ToTable(TableNames.NumberSequences);

        builder.Property(x => x.Code)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(64))
            .IsRequired();

        builder.Property(x => x.Prefix)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(16));

        builder.Property(x => x.LastNumber)
            .HasColumnType(SqlDbType.Int.GetSqlType())
            .IsRequired();

        builder.Property(x => x.Padding)
            .HasColumnType(SqlDbType.Int.GetSqlType())
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("IX_NumberSequences_Code");
    }
    #endregion Methods
}