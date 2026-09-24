using Accounting.Core.Business.Interfaces;
using Accounting.Shared.Markers;
using Microsoft.AspNetCore.DataProtection;

namespace Accounting.Core.DataAccess.Services;

public class DataProtectionSecretProtector(IDataProtectionProvider provider) : ISecretProtector, ISingletonService
{
    #region Fields
    private readonly IDataProtector _protector = provider.CreateProtector("Accounting.Secrets.v1");

    #endregion Fields
    
    #region Operations
    public string Protect(string plainText)
    {
        return _protector.Protect(plainText);
    }

    public string Unprotect(string protectedText)
    {
        return _protector.Unprotect(protectedText);
    }
    #endregion Operations
}