using Accounting.Core.DataAccess.Persistence.Constants;
using Accounting.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data;

namespace Accounting.Core.DataAccess.Persistence.Master.Configurations;

public sealed class UserCompanyConfiguration : BaseEntityConfiguration<UserCompany>
{
    #region Methods
    protected override void ConfigureEntity(EntityTypeBuilder<UserCompany> builder)
    {
        builder.ToTable(TableNames.UserCompanies);

        builder.Property(x => x.UserId)
            .HasColumnType(SqlDbType.UniqueIdentifier.GetSqlType())
            .IsRequired();

        builder.Property(x => x.CompanyId)
            .HasColumnType(SqlDbType.UniqueIdentifier.GetSqlType())
            .IsRequired();

        builder.Property(x => x.IsDefault)
            .HasColumnType(SqlDbType.Bit.GetSqlType())
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(x => x.UserCompanies)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Company)
            .WithMany(x => x.UserCompanies)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId , x.CompanyId })
            .IsUnique()
            .HasDatabaseName("IX_UserCompanies_UserId_CompanyId");

        builder.HasIndex(x => x.UserId)
            .IsUnique()
            .HasFilter("[IsDefault] = 1")
            .HasDatabaseName("IX_UserCompanies_UserId_Default");
    }
    #endregion Methods
}