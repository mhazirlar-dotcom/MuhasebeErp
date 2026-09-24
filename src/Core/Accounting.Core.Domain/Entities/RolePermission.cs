using Accounting.Core.Domain.Common;

namespace Accounting.Core.Domain.Entities;

public sealed class RolePermission : BaseEntity
{
    #region Properties
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    #endregion Properties

    #region Relations
    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
    #endregion Relations
}