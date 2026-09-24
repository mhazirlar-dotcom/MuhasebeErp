using Accounting.Core.Domain.Common;

namespace Accounting.Core.Domain.Entities;

public sealed class NumberSequence : AuditableEntity
{
    #region Properties
    public string Code { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public int LastNumber { get; set; } = 0;
    public int Padding { get; set; } = 6;
    #endregion Properties
}