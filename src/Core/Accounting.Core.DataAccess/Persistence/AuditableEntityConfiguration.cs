using Accounting.Core.DataAccess.Persistence.Constants;
using Accounting.Core.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data;

namespace Accounting.Core.DataAccess.Persistence;

public abstract class AuditableEntityConfiguration<T> : BaseEntityConfiguration<T>
    where T : AuditableEntity
{
    #region Methods
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.CreatedAt)
            .HasColumnType(SqlDbType.DateTime2.GetSqlType())
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(128))
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType(SqlDbType.DateTime2.GetSqlType());

        builder.Property(x => x.UpdatedBy)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(128));
    }
    #endregion Methods
}