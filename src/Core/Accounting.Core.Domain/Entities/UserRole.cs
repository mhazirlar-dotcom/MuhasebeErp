using Accounting.Core.Domain.Common;

namespace Accounting.Core.Domain.Entities;

public sealed class UserRole : BaseEntity
{
    #region Properties
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    #endregion Properties

    #region Relations
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
    #endregion Relations
}