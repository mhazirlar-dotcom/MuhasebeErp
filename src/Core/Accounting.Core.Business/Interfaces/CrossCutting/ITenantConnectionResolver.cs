namespace Accounting.Core.Business.Interfaces;

public interface ITenantConnectionResolver
{
    #region Operations
    string ResolveMasterConnection();
    string ResolveCompanyConnection();
    #endregion Operations
}