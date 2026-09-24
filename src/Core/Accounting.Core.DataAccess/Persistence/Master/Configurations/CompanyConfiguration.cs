using Accounting.Core.DataAccess.Persistence.Constants;
using Accounting.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data;

namespace Accounting.Core.DataAccess.Persistence.Master.Configurations;

public sealed class CompanyConfiguration : AuditableEntityConfiguration<Company>
{
    #region Methods
    protected override void ConfigureEntity(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable(TableNames.Companies);

        builder.Property(x => x.ShortName)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(64))
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(256))
            .IsRequired();

        builder.Property(x => x.IdentityNumber)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(11));

        builder.Property(x => x.TaxNumber)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(10))
            .IsRequired();

        builder.Property(x => x.LegalStatus)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(32));

        builder.Property(x => x.LegalNature)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(32));

        builder.Property(x => x.Description)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(1024));

        builder.Property(x => x.TaxOffice)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(128));

        builder.Property(x => x.FoundationDate)
            .HasColumnType(SqlDbType.DateTime2.GetSqlType());

        builder.Property(x => x.ActivityCode)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(32));

        builder.Property(x => x.SocialSecurityInstitution)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(128));

        builder.Property(x => x.ProfessionalOrganization)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(128));

        builder.Property(x => x.ProfessionalOrganizationMemberNumber)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(32));

        builder.Property(x => x.TradeRegistryOffice)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(128));

        builder.Property(x => x.TradeRegistryNumber)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(32));

        builder.Property(x => x.RegistryNumber)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(32));

        builder.Property(x => x.WithholdingDeclarationMethod)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(32));

        builder.Property(x => x.Create302RecordForWithholding)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(32));

        builder.Property(x => x.PayrollCutoffDate)
            .HasColumnType(SqlDbType.DateTime2.GetSqlType());

        builder.Property(x => x.MerisNumber)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(32));

        builder.Property(x => x.TaxAuthorityUsername)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(128));

        builder.Property(x => x.IsSpecialTaxpayer)
            .HasColumnType(SqlDbType.Bit.GetSqlType())
            .IsRequired();

        builder.Property(x => x.SendReceiptDescriptionForDbs)
            .HasColumnType(SqlDbType.Bit.GetSqlType())
            .IsRequired();

        builder.Property(x => x.SendReceiptDescriptionForLedger)
            .HasColumnType(SqlDbType.Bit.GetSqlType())
            .IsRequired();

        builder.Property(x => x.AdminOnlyAccess)
            .HasColumnType(SqlDbType.Bit.GetSqlType())
            .IsRequired();

        builder.Property(x => x.ServerName)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(256))
            .IsRequired();

        builder.Property(x => x.DatabaseName)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(128))
            .IsRequired();

        builder.Property(x => x.DbUserName)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(128));

        builder.Property(x => x.DbPasswordEncrypted)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(1024));

        builder.Property(x => x.IntegratedSecurity)
            .HasColumnType(SqlDbType.Bit.GetSqlType())
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnType(SqlDbType.Bit.GetSqlType())
            .IsRequired();

        builder.HasIndex(x => x.ShortName)
            .IsUnique()
            .HasDatabaseName("IX_Companies_ShortName");

        builder.HasIndex(x => x.DatabaseName)
            .IsUnique()
            .HasDatabaseName("IX_Companies_DatabaseName");

        builder.HasIndex(x => x.IdentityNumber)
            .IsUnique()
            .HasFilter("[IdentityNumber] IS NOT NULL AND [IdentityNumber] <> ''")
            .HasDatabaseName("IX_Companies_IdentityNumber");

        builder.HasIndex(x => x.TaxNumber)
            .IsUnique()
            .HasDatabaseName("IX_Companies_TaxNumber");
    }
    #endregion Methods
}