using Accounting.Core.Domain.Common;

namespace Accounting.Core.Domain.Entities;

public sealed class RefreshToken : AuditableEntity
{
    #region Properties
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; } = null;
    public string ReplacedByTokenHash { get; set; } = string.Empty;
    public string CreatedByIp { get; set; } = string.Empty;
    #endregion Properties

    #region Relations
    public User User { get; set; } = null!;
    #endregion Relations
}