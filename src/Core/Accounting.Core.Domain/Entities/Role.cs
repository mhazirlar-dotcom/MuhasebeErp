using Accounting.Core.Domain.Common;

namespace Accounting.Core.Domain.Entities;

public sealed class Role : AuditableEntity
{
    #region Properties
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    #endregion Properties

    #region Relations
    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
    #endregion Relations
}