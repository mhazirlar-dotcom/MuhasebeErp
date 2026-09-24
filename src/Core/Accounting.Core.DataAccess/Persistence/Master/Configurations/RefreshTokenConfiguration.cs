using Accounting.Core.DataAccess.Persistence.Constants;
using Accounting.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data;

namespace Accounting.Core.DataAccess.Persistence.Master.Configurations;

public sealed class RefreshTokenConfiguration : AuditableEntityConfiguration<RefreshToken>
{
    #region Methods
    protected override void ConfigureEntity(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable(TableNames.RefreshTokens);

        builder.Property(x => x.UserId)
            .HasColumnType(SqlDbType.UniqueIdentifier.GetSqlType())
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(64))
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .HasColumnType(SqlDbType.DateTime2.GetSqlType())
            .IsRequired();

        builder.Property(x => x.IsRevoked)
            .HasColumnType(SqlDbType.Bit.GetSqlType())
            .IsRequired();

        builder.Property(x => x.RevokedAt)
            .HasColumnType(SqlDbType.DateTime2.GetSqlType());

        builder.Property(x => x.ReplacedByTokenHash)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(64));

        builder.Property(x => x.CreatedByIp)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(45));

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.TokenHash)
            .IsUnique()
            .HasDatabaseName("IX_RefreshTokens_TokenHash");

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_RefreshTokens_UserId");

        builder.HasIndex(x => x.ExpiresAt)
            .HasDatabaseName("IX_RefreshTokens_ExpiresAt");
    }
    #endregion Methods
}