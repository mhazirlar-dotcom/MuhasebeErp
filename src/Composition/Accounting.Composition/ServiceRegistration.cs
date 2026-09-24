using Accounting.Core.Business.Interfaces;
using Accounting.Core.Business.Validators.Master;
using Accounting.Core.DataAccess.Persistence.Companies;
using Accounting.Core.DataAccess.Persistence.Master;
using Accounting.Shared.Markers;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using System.Reflection;

namespace Accounting.Composition;

public static class ServiceRegistration
{
    #region Operations
    public static IServiceCollection AddAccountingLocal(this IServiceCollection services , IConfiguration configuration , params Assembly[] additionalAssemblies)
    {
        return AddAccounting(services , configuration , additionalAssemblies , isLocal: true);
    }

    public static IServiceCollection AddAccountingApi(this IServiceCollection services , IConfiguration configuration , params Assembly[] additionalAssemblies)
    {
        return AddAccounting(services , configuration , additionalAssemblies , isLocal: false);
    }
    #endregion Operations

    #region Framework
    private static IServiceCollection AddAccounting(IServiceCollection services , IConfiguration configuration , Assembly[] additionalAssemblies , bool isLocal)
    {
        services.AddDataProtection();
        AddCoreBusinessValidators(services);

        if (!isLocal)
        {
            AddMasterDbContext(services , configuration);
            AddCompanyDbContext(services);
        }

        RegisterServices(services , additionalAssemblies , includeLocalSingletons: isLocal);

        return services;
    }
    #endregion Framework

    #region Core Business
    private static void AddCoreBusinessValidators(IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CompanyValidator>();
    }
    #endregion Core Business

    #region Core DataAccess
    private static void AddMasterDbContext(IServiceCollection services , IConfiguration configuration)
    {
        string masterConnection = configuration.GetConnectionString("Master") ?? string.Empty;

        services.AddDbContext<MasterDbContext>(options =>
            options.UseSqlServer(masterConnection));
    }

    private static void AddCompanyDbContext(IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>((sp , options) =>
        {
            ITenantConnectionResolver resolver = sp.GetRequiredService<ITenantConnectionResolver>();
            options.UseSqlServer(resolver.ResolveCompanyConnection());
        });
    }
    #endregion Core DataAccess

    #region Service Scanning
    private static void RegisterServices(IServiceCollection services , Assembly[] additionalAssemblies , bool includeLocalSingletons)
    {
        services.Scan(scan =>
        {
            RegisterAssembly(scan.FromApplicationDependencies() , includeLocalSingletons);

            if (additionalAssemblies.Length > 0)
            {
                RegisterAssembly(scan.FromAssemblies(additionalAssemblies) , includeLocalSingletons);
            }
        });
    }

    private static void RegisterAssembly(IImplementationTypeSelector selector , bool includeLocalSingletons)
    {
        AddWithLifetime<IScopedService>(selector , ServiceLifetime.Scoped);
        AddWithLifetime<ITransientService>(selector , ServiceLifetime.Transient);
        AddWithLifetime<ISingletonService>(selector , ServiceLifetime.Singleton);

        if (includeLocalSingletons)
        {
            AddWithLifetime<ILocalSingletonService>(selector , ServiceLifetime.Singleton);
        }
    }

    private static void AddWithLifetime<TMarker>(IImplementationTypeSelector selector , ServiceLifetime lifetime) where TMarker : class
    {
        ILifetimeSelector typeSelector = selector
            .AddClasses(classes => classes.AssignableTo<TMarker>())
            .AsSelfWithInterfaces();

        _ = lifetime switch
        {
            ServiceLifetime.Scoped => typeSelector.WithScopedLifetime(),
            ServiceLifetime.Transient => typeSelector.WithTransientLifetime(),
            ServiceLifetime.Singleton => typeSelector.WithSingletonLifetime(),
            _ => throw new ArgumentOutOfRangeException(nameof(lifetime))
        };
    }
    #endregion Service Scanning
}