using Accounting.Core.Domain.Common;

namespace Accounting.Core.Domain.Entities;

public sealed class UserCompany : BaseEntity
{
    #region Properties
    public Guid UserId { get; set; }
    public Guid CompanyId { get; set; }
    public bool IsDefault { get; set; } = false;
    #endregion Properties

    #region Relations
    public User User { get; set; } = null!;
    public Company Company { get; set; } = null!;
    #endregion Relations
}