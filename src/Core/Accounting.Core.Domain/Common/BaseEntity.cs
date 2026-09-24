namespace Accounting.Core.Domain.Common;

public abstract class BaseEntity
{
    #region Properties
    public Guid Id { get; set; } = Guid.NewGuid();
    #endregion Properties
}
