using Accounting.Core.Business.Interfaces;
using Accounting.Shared.Markers;

namespace Accounting.Core.Business.Services;

public class SystemClock : IClock, ISingletonService
{
    #region Properties
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
    #endregion Properties
}