using Accounting.Core.Domain.Common;

namespace Accounting.Core.Domain.Entities;

public sealed class LookupValue : AuditableEntity
{
    #region Properties
    public string Type { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? ParentId { get; set; } = null;
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public string DisplayText => $"{Code} - {Name}";
    #endregion Properties

    #region Relations
    public LookupValue? Parent { get; set; } = null;
    public ICollection<LookupValue> Children { get; set; } = [];
    #endregion Relations
}