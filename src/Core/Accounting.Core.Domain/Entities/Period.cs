using Accounting.Core.Domain.Common;

namespace Accounting.Core.Domain.Entities;

public sealed class Period : AuditableEntity
{
    #region Properties
    // --- Ortak ---
    public Guid CompanyId { get; set; }
    public int Year { get; set; }
    public int MonthNumber { get; set; } = 1;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsClosed { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime FinalizationDate { get; set; } = DateTime.MinValue;
    public string AccountingMethod { get; set; } = string.Empty;
    public int FirmClass { get; set; } = 1;

    // --- Geçmiş Yıl Zararları ---
    public decimal OtherLoss { get; set; } = 0m;
    public decimal ExceptionLoss { get; set; } = 0m;

    // --- 1. Sınıf ---
    public decimal GrossWage { get; set; } = 0m;
    public decimal NetWage { get; set; } = 0m;
    public string VatRate { get; set; } = string.Empty;
    public decimal VatAmount { get; set; } = 0m;
    public decimal StorageRate { get; set; } = 0m;
    public decimal StorageAmount { get; set; } = 0m;
    public int JournalStartNumber { get; set; } = 0;
    public string DeclarationType { get; set; } = string.Empty;
    public string VoucherSortMode { get; set; } = string.Empty;
    public bool RenumberVouchers { get; set; } = false;
    public string VoucherNumberLength { get; set; } = string.Empty;
    public bool UseForeignCurrency { get; set; } = false;
    public string SystemCurrency { get; set; } = string.Empty;
    public string ExchangeRateMode { get; set; } = string.Empty;

    // --- 2. Sınıf ---
    public string LedgerType { get; set; } = string.Empty;
    public int IncomeStartNumber { get; set; } = 0;
    public int ExpenseStartNumber { get; set; } = 0;
    public decimal CarryoverVat { get; set; } = 0m;
    public bool IsVatTaxpayer { get; set; } = false;
    public bool UseDbsStockLedger { get; set; } = false;
    #endregion Properties

    #region Computed
    public string DisplayName => $"{StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy}";
    public string FirmClassDisplay => FirmClass == 1 ? "1. Sınıf" : "2. Sınıf";
    public bool IsClass1 => FirmClass == 1;
    public bool IsClass2 => FirmClass == 2;
    #endregion Computed
}