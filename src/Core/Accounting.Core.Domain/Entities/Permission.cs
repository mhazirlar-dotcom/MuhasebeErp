using Accounting.Core.Domain.Common;

namespace Accounting.Core.Domain.Entities;

public sealed class Permission : AuditableEntity
{
    #region Properties
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    #endregion Properties

    #region Relations
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
    #endregion Relations
}