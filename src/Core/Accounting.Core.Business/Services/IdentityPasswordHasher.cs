using Accounting.Core.Business.Interfaces;
using Accounting.Shared.Markers;
using Microsoft.AspNetCore.Identity;

namespace Accounting.Core.Business.Services;

public class IdentityPasswordHasher : IPasswordHasher, ISingletonService
{
    #region Fields
    private readonly PasswordHasher<object> _hasher = new();
    private static readonly object _dummyUser = new();
    #endregion Fields

    #region Operations
    public string Hash(string password)
    {
        return _hasher.HashPassword(_dummyUser , password);
    }

    public bool Verify(string password , string hash)
    {
        var result = _hasher.VerifyHashedPassword(_dummyUser, hash, password);
        return result == PasswordVerificationResult.Success
            || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
    #endregion Operations
}