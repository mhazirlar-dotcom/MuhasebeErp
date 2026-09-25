using Accounting.Core.DataAccess.Persistence.Constants;
using Accounting.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data;

namespace Accounting.Core.DataAccess.Persistence.Master.Configurations;

public sealed class PeriodConfiguration : AuditableEntityConfiguration<Period>
{
    #region Methods
    protected override void ConfigureEntity(EntityTypeBuilder<Period> builder)
    {
        builder.ToTable(TableNames.Periods , t => t.HasCheckConstraint(
            "CK_Periods_DateRange" , "[EndDate] > [StartDate]"));

        // --- Ortak ---
        builder.Property(x => x.CompanyId)
            .HasColumnType(SqlDbType.UniqueIdentifier.GetSqlType())
            .IsRequired();

        builder.Property(x => x.Year)
            .HasColumnType(SqlDbType.Int.GetSqlType())
            .IsRequired();

        builder.Property(x => x.MonthNumber)
            .HasColumnType(SqlDbType.Int.GetSqlType())
            .IsRequired();

        builder.Property(x => x.StartDate)
            .HasColumnType(SqlDbType.DateTime2.GetSqlType())
            .IsRequired();

        builder.Property(x => x.EndDate)
            .HasColumnType(SqlDbType.DateTime2.GetSqlType())
            .IsRequired();

        builder.Property(x => x.IsClosed)
            .HasColumnType(SqlDbType.Bit.GetSqlType())
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnType(SqlDbType.Bit.GetSqlType())
            .IsRequired();

        builder.Property(x => x.FinalizationDate)
            .HasColumnType(SqlDbType.DateTime2.GetSqlType());

        builder.Property(x => x.AccountingMethod)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(32));

        builder.Property(x => x.FirmClass)
            .HasColumnType(SqlDbType.Int.GetSqlType())
            .IsRequired();

        // --- Geçmiş Yıl Zararları ---
        builder.Property(x => x.OtherLoss)
            .HasColumnType(SqlDbType.Decimal.GetSqlType(18 , 2))
            .IsRequired();

        builder.Property(x => x.ExceptionLoss)
            .HasColumnType(SqlDbType.Decimal.GetSqlType(18 , 2))
            .IsRequired();

        // --- 1. Sınıf ---
        builder.Property(x => x.GrossWage)
            .HasColumnType(SqlDbType.Decimal.GetSqlType(18 , 2))
            .IsRequired();

        builder.Property(x => x.NetWage)
            .HasColumnType(SqlDbType.Decimal.GetSqlType(18 , 2))
            .IsRequired();

        builder.Property(x => x.VatRate)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(32));

        builder.Property(x => x.VatAmount)
            .HasColumnType(SqlDbType.Decimal.GetSqlType(18 , 2))
            .IsRequired();

        builder.Property(x => x.StorageRate)
            .HasColumnType(SqlDbType.Decimal.GetSqlType(18 , 4))
            .IsRequired();

        builder.Property(x => x.StorageAmount)
            .HasColumnType(SqlDbType.Decimal.GetSqlType(18 , 2))
            .IsRequired();

        builder.Property(x => x.JournalStartNumber)
            .HasColumnType(SqlDbType.Int.GetSqlType())
            .IsRequired();

        builder.Property(x => x.DeclarationType)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(32));

        builder.Property(x => x.VoucherSortMode)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(32));

        builder.Property(x => x.RenumberVouchers)
            .HasColumnType(SqlDbType.Bit.GetSqlType())
            .IsRequired();

        builder.Property(x => x.VoucherNumberLength)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(8));

        builder.Property(x => x.UseForeignCurrency)
            .HasColumnType(SqlDbType.Bit.GetSqlType())
            .IsRequired();

        builder.Property(x => x.SystemCurrency)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(8));

        builder.Property(x => x.ExchangeRateMode)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(32));

        // --- 2. Sınıf ---
        builder.Property(x => x.LedgerType)
            .HasColumnType(SqlDbType.NVarChar.GetSqlType(32));

        builder.Property(x => x.IncomeStartNumber)
            .HasColumnType(SqlDbType.Int.GetSqlType())
            .IsRequired();

        builder.Property(x => x.ExpenseStartNumber)
            .HasColumnType(SqlDbType.Int.GetSqlType())
            .IsRequired();

        builder.Property(x => x.CarryoverVat)
            .HasColumnType(SqlDbType.Decimal.GetSqlType(18 , 2))
            .IsRequired();

        builder.Property(x => x.IsVatTaxpayer)
            .HasColumnType(SqlDbType.Bit.GetSqlType())
            .IsRequired();

        builder.Property(x => x.UseDbsStockLedger)
            .HasColumnType(SqlDbType.Bit.GetSqlType())
            .IsRequired();

        // --- Computed ---
        builder.Ignore(x => x.DisplayName);
        builder.Ignore(x => x.FirmClassDisplay);
        builder.Ignore(x => x.IsClass1);
        builder.Ignore(x => x.IsClass2);
        builder.Ignore(x => x.StartDateNullable);
        builder.Ignore(x => x.EndDateNullable);
        builder.Ignore(x => x.FinalizationDateNullable);

        // --- Relations ---
        builder.HasOne<Company>()
            .WithMany(x => x.Periods)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- Index ---
        builder.HasIndex(x => new { x.CompanyId , x.Year })
            .IsUnique()
            .HasDatabaseName("IX_Periods_CompanyId_Year");
    }
    #endregion Methods
}