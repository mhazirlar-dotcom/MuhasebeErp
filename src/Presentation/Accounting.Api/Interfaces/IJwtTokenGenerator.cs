using Accounting.Core.Domain.Entities;

namespace Accounting.Api.Interfaces;

public interface IJwtTokenGenerator
{
    #region Operations
    (string Token , DateTime ExpiresAt) GenerateAccessToken(User user);
    #endregion Operations
}