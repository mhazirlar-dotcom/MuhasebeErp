using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accounting.Core.DataAccess.Persistence.Master.Migrations
{
    /// <inheritdoc />
    public partial class ExpandPeriodFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Periods");

            migrationBuilder.AddColumn<string>(
                name: "AccountingMethod",
                table: "Periods",
                type: "nvarchar(32)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "CarryoverVat",
                table: "Periods",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DeclarationType",
                table: "Periods",
                type: "nvarchar(32)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExchangeRateMode",
                table: "Periods",
                type: "nvarchar(32)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ExpenseStartNumber",
                table: "Periods",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinalizationDate",
                table: "Periods",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "FirmClass",
                table: "Periods",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossWage",
                table: "Periods",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "IncomeStartNumber",
                table: "Periods",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Periods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVatTaxpayer",
                table: "Periods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "JournalStartNumber",
                table: "Periods",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LedgerType",
                table: "Periods",
                type: "nvarchar(32)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MonthNumber",
                table: "Periods",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "NetWage",
                table: "Periods",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "RenumberVouchers",
                table: "Periods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "StorageAmount",
                table: "Periods",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "StorageRate",
                table: "Periods",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SystemCurrency",
                table: "Periods",
                type: "nvarchar(8)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "UseDbsStockLedger",
                table: "Periods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UseForeignCurrency",
                table: "Periods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "VatAmount",
                table: "Periods",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "VatRate",
                table: "Periods",
                type: "nvarchar(32)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VoucherNumberLength",
                table: "Periods",
                type: "nvarchar(8)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VoucherSortMode",
                table: "Periods",
                type: "nvarchar(32)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountingMethod",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "CarryoverVat",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "DeclarationType",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "ExchangeRateMode",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "ExpenseStartNumber",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "FinalizationDate",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "FirmClass",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "GrossWage",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "IncomeStartNumber",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "IsVatTaxpayer",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "JournalStartNumber",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "LedgerType",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "MonthNumber",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "NetWage",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "RenumberVouchers",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "StorageAmount",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "StorageRate",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "SystemCurrency",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "UseDbsStockLedger",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "UseForeignCurrency",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "VatAmount",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "VatRate",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "VoucherNumberLength",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "VoucherSortMode",
                table: "Periods");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Periods",
                type: "nvarchar(64)",
                nullable: false,
                defaultValue: "");
        }
    }
}
