using Accounting.Core.Business.Interfaces;

namespace Accounting.Core.DataAccess.Persistence.DesignTime;

internal sealed class DesignTimeClock : IClock
{
    #region Properties
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
    #endregion Properties
}