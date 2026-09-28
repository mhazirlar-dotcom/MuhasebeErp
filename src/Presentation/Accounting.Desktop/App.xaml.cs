using Accounting.Composition;
using Accounting.Core.Business.Interfaces.Services;
using Accounting.Desktop.Extensions;
using Accounting.Desktop.Services;
using Accounting.Desktop.ViewModels;
using Accounting.Desktop.Windows;
using Accounting.Shared.Results;
using DevExpress.Xpf.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Windows;

namespace Accounting.Desktop;

public partial class App : Application
{
    #region Properties
    public static IServiceProvider Services { get; private set; } = null!;
    private static IHost _host = null!;
    private static ApiProcessManager _apiProcessManager = null!;
    #endregion Properties

    #region Operations
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ApplicationThemeHelper.ApplicationThemeName = Theme.Office2019Colorful.Name;

        _host = CreateHost();
        await _host.StartAsync();

        Services = _host.Services;

        Log.Information("Uygulama başladı.");

        try
        {
            _apiProcessManager = new ApiProcessManager();
            await _apiProcessManager.StartAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex , "API başlatılamadı.");
            MessageBox.Show(
                $"API başlatılamadı:\n\n{ex.GetType().Name}\n{ex.Message}" ,
                "Başlangıç Hatası" , MessageBoxButton.OK , MessageBoxImage.Error);
            Current.Shutdown();
            return;
        }

        await RunStartupFlowAsync();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        Log.Information("Uygulama kapanıyor.");

        try
        {
            _apiProcessManager?.Dispose();
            Log.Information("[App.OnExit] API process temizlendi.");
        }
        catch (Exception ex)
        {
            Log.Warning(ex , "[App.OnExit] API process temizlenemedi.");
        }

        await Log.CloseAndFlushAsync();

        if (_host is IDisposable disposable)
        {
            disposable.Dispose();
        }

        base.OnExit(e);
    }
    #endregion Operations

    #region Helpers
    private static IHost CreateHost()
    {
        return Host.CreateDefaultBuilder()
            .UseSerilog((context , services , configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services);
            })
            .ConfigureServices((context , services) =>
            {
                services.AddAccountingLocal(
                    context.Configuration ,
                    typeof(LoginViewModel).Assembly);

                services.AddDesktopHttpClients(context.Configuration);

                services.AddTransient<Views.WelcomeView>();
            })
            .Build();
    }

    private static async Task RunStartupFlowAsync()
    {
        try
        {
            using (IServiceScope scope = Services.CreateScope())
            {
                IAuthService authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

                Result<bool> hasUserResult = await authService.HasAnyUserAsync();

                if (hasUserResult.IsFailure)
                {
                    MessageBox.Show(
                        $"Başlangıç kontrolü başarısız: {hasUserResult.Message}" ,
                        "Hata" , MessageBoxButton.OK , MessageBoxImage.Error);
                    Current.Shutdown();
                    return;
                }

                if (!hasUserResult.Data)
                {
                    bool setupCompleted = ShowFirstSetup();
                    if (!setupCompleted)
                    {
                        Current.Shutdown();
                        return;
                    }
                }
            }

            bool continueLoop = true;
            while (continueLoop)
            {
                bool loginSucceeded = ShowLogin();
                if (!loginSucceeded)
                {
                    Current.Shutdown();
                    return;
                }

                bool logoutRequested = ShowShell();

                if (logoutRequested)
                {
                    Log.Information("Kullanıcı çıkış yaptı. Login ekranına dönülüyor.");
                    continueLoop = true;
                }
                else
                {
                    continueLoop = false;
                }
            }

            Current.Shutdown();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex , "Başlangıç akışı sırasında kritik hata.");
            MessageBox.Show(
                $"Başlangıç hatası:\n\n{ex.GetType().Name}\n{ex.Message}\n\n{ex.StackTrace}" ,
                "Kritik Hata" , MessageBoxButton.OK , MessageBoxImage.Error);
            Current.Shutdown();
        }
    }

    private static bool ShowFirstSetup()
    {
        using IServiceScope scope = Services.CreateScope();

        FirstSetupViewModel viewModel = scope.ServiceProvider.GetRequiredService<FirstSetupViewModel>();
        FirstSetupWindow window = new(viewModel);

        bool? result = window.ShowDialog();
        return result == true;
    }

    private static bool ShowLogin()
    {
        using IServiceScope scope = Services.CreateScope();

        LoginViewModel viewModel = scope.ServiceProvider.GetRequiredService<LoginViewModel>();
        LoginWindow window = new(viewModel);

        bool? result = window.ShowDialog();
        return result == true;
    }

    private static bool ShowShell()
    {
        using IServiceScope scope = Services.CreateScope();

        ShellViewModel shellViewModel = scope.ServiceProvider.GetRequiredService<ShellViewModel>();
        ShellWindow shellWindow = new(shellViewModel);

        Current.MainWindow = shellWindow;

        bool? result = shellWindow.ShowDialog();

        return result == true;
    }
    #endregion Helpers
}