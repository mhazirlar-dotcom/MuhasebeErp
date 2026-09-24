namespace Accounting.Core.Domain.Common;

public interface IAuditable
{
    #region Audit
    DateTime CreatedAt { get; set; }
    string CreatedBy { get; set; }
    DateTime UpdatedAt { get; set; }
    string UpdatedBy { get; set; }
    #endregion Audit
}