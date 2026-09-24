using Accounting.Core.Domain.Common;

namespace Accounting.Core.Domain.Entities;

public sealed class User : AuditableEntity
{
    #region Properties
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime LastLoginAt { get; set; } = DateTime.MinValue;
    #endregion Properties

    #region Relations
    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<UserCompany> UserCompanies { get; set; } = [];
    #endregion Relations
}