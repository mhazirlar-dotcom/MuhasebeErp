namespace Accounting.Core.Business.Interfaces;

public interface ISecretProtector
{
    #region Operations
    string Protect(string plainText);
    string Unprotect(string protectedText);
    #endregion Operations
}