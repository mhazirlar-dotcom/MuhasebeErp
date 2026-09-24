using Accounting.Core.DataAccess.Persistence.Constants;
using Accounting.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data;

namespace Accounting.Core.DataAccess.Persistence.Master.Configurations;

public sealed class UserConfiguration : AuditableEntityConfiguration<User>
{
    #region Methods
    protected override void ConfigureEntity(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(TableNames.Users);

        builder.Property(x => x.UserName)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(64))
            .IsRequired();

        builder.Property(x => x.FullName)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(128))
            .IsRequired();

        builder.Property(x => x.Email)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(256))
            .IsRequired();

        builder.Property(x => x.Phone)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(32));

        builder.Property(x => x.PasswordHash)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(512))
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnType(SqlDbType.Bit.GetSqlType())
            .IsRequired();

        builder.Property(x => x.LastLoginAt)
            .HasColumnType(SqlDbType.DateTime2.GetSqlType())
            .IsRequired();

        builder.HasIndex(x => x.UserName)
            .IsUnique()
            .HasDatabaseName("IX_Users_UserName");

        builder.HasIndex(x => x.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");
    }
    #endregion Methods
}