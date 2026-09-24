namespace Accounting.Core.Domain.Common;

public abstract class AuditableEntity : BaseEntity, IAuditable
{
    #region Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.MinValue;
    public string UpdatedBy { get; set; } = string.Empty;
    #endregion Audit
}