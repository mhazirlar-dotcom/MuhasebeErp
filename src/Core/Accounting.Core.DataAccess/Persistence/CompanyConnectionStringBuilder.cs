using Accounting.Core.Business.Options;
using Accounting.Core.Domain.Entities;

namespace Accounting.Core.DataAccess.Persistence;

public static class CompanyConnectionStringBuilder
{
    #region Operations
    public static string BuildFromOptions(CompanyDatabaseOptions options , Company company)
    {
        return Build(company.ServerName , company.DatabaseName , company.IntegratedSecurity , company.DbUserName , options.DbPassword);
    }

    public static string BuildFromCompany(Company company , string decryptedPassword)
    {
        return Build(company.ServerName , company.DatabaseName , company.IntegratedSecurity , company.DbUserName , decryptedPassword);
    }
    #endregion Operations

    #region Helpers
    private static string Build(string serverName , string databaseName , bool integratedSecurity , string userName , string password)
    {
        if (integratedSecurity)
        {
            return $"Server={serverName};Database={databaseName};Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True";
        }

        return $"Server={serverName};Database={databaseName};User Id={userName};Password={password};TrustServerCertificate=True;MultipleActiveResultSets=True";
    }
    #endregion Helpers
}