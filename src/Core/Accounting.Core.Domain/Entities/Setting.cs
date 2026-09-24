using Accounting.Core.Domain.Common;

namespace Accounting.Core.Domain.Entities;

public sealed class Setting : AuditableEntity
{
    #region Properties
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    #endregion Properties
}