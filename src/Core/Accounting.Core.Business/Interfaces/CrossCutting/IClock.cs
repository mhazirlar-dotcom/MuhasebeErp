namespace Accounting.Core.Business.Interfaces;

public interface IClock
{
    #region Properties
    DateTime UtcNow { get; }
    DateTime Now { get; }
    #endregion Properties
}