namespace Accounting.Core.Business.Interfaces;

public interface IPasswordHasher
{
    #region Operations
    string Hash(string password);
    bool Verify(string password , string hash);
    #endregion Operations
}