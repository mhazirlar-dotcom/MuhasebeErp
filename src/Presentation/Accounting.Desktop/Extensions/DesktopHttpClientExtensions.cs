using Accounting.Core.Business.Interfaces.Services;
using Accounting.Desktop.Services.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Accounting.Desktop.Extensions;

public static class DesktopHttpClientExtensions
{
    #region Constants
    private const string RefreshClientName = "AuthRefreshClient";
    #endregion Constants

    #region Operations
    public static IServiceCollection AddDesktopHttpClients(this IServiceCollection services , IConfiguration configuration)
    {
        string apiBaseUrl = configuration["Api:BaseUrl"] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(apiBaseUrl))
        {
            return services;
        }

        Uri baseAddress = new(apiBaseUrl);

        ConfigureMainHttpClient(services , baseAddress);
        ConfigureRefreshHttpClient(services , baseAddress);
        RegisterHttpServices(services);

        return services;
    }
    #endregion Operations

    #region Helpers
    private static void ConfigureMainHttpClient(IServiceCollection services , Uri baseAddress)
    {
        services.AddTransient<AuthTokenHandler>();

        services.AddHttpClient(Options.DefaultName)
            .ConfigureHttpClient(client => client.BaseAddress = baseAddress)
            .AddHttpMessageHandler<AuthTokenHandler>();
    }

    private static void ConfigureRefreshHttpClient(IServiceCollection services , Uri baseAddress)
    {
        services.AddHttpClient(RefreshClientName)
            .ConfigureHttpClient(client => client.BaseAddress = baseAddress);
    }

    private static void RegisterHttpServices(IServiceCollection services)
    {
        services.AddTransient<IAuthService , HttpAuthService>();
        services.AddTransient<ICompanyService , HttpCompanyService>();
        services.AddTransient<IPeriodService , HttpPeriodService>();
        services.AddTransient<ILookupValueService , HttpLookupValueService>();
    }
    #endregion Helpers
}