using Accounting.Core.Business.Interfaces;
using Accounting.Core.Business.Interfaces.CrossCutting;
using Accounting.Core.DataAccess.Persistence;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Markers;
using Microsoft.Extensions.Configuration;

namespace Accounting.Core.DataAccess.Services;

public class TenantConnectionResolver(IConfiguration configuration , ITenantContext tenant , ISecretProtector protector) : ITenantConnectionResolver, IScopedService
{
    #region Operations
    public string ResolveMasterConnection()
    {
        return configuration.GetConnectionString("Master") ?? string.Empty;
    }

    public string ResolveCompanyConnection()
    {
        if (!tenant.HasCompany)
        {
            return ResolveMasterConnection();
        }

        Company company = tenant.CurrentCompany;

        string password = string.IsNullOrEmpty(company.DbPasswordEncrypted)
            ? string.Empty
            : protector.Unprotect(company.DbPasswordEncrypted);

        return CompanyConnectionStringBuilder.BuildFromCompany(company , password);
    }
    #endregion Operations
}