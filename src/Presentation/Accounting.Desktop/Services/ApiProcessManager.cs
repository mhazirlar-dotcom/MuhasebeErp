using Serilog;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;

namespace Accounting.Desktop.Services;

public sealed class ApiProcessManager : IDisposable
{
    #region Constants
    private const string ApiBaseUrl = "http://localhost:5233";
    private const string HealthCheckUrl = "http://localhost:5233/api/auth/has-user";
    private const string DebugPath = @"src\Presentation\Accounting.Api\bin\Debug\net10.0\win-x64\Accounting.Api.exe";
    private const string DebugPathFallback = @"src\Presentation\Accounting.Api\bin\Debug\net10.0\Accounting.Api.exe";
    private const string ReleasePath = @"src\Presentation\Accounting.Api\bin\Release\net10.0\win-x64\Accounting.Api.exe";
    private const int ApiPort = 5233;
    private const int HealthCheckIntervalMs = 100;
    private const int HealthCheckTimeoutSeconds = 90;
    #endregion Constants

    #region Fields
    private Process? _apiProcess;
    private bool _disposed = false;
    #endregion Fields

    #region Operations
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (await IsApiHealthyAsync(cancellationToken))
        {
            Log.Information("[ApiProcessManager] API zaten çalışıyor ve sağlıklı, başlatma atlandı.");
            return;
        }

        if (IsPortInUse())
        {
            bool becameHealthy = await WaitForApiReadyAsync(cancellationToken);

            if (becameHealthy)
            {
                Log.Information("[ApiProcessManager] Port zaten kullanımdaydı ve API sağlıklı hale geldi.");
                return;
            }

            throw new InvalidOperationException(
                $"Port {ApiPort} başka bir process tarafından kullanılıyor ama API sağlık kontrolü başarısız.\n\n" +
                "Çözüm: Task Manager'da 'Accounting.Api.exe' process'lerini sonlandırın veya\n" +
                $"komut: taskkill /F /IM Accounting.Api.exe\n\n" +
                $"Adres: {HealthCheckUrl}");
        }

        string apiPath = ResolveApiExecutablePath();

        if (!File.Exists(apiPath))
        {
            throw new FileNotFoundException(
                $"API executable bulunamadı: {apiPath}\n\n" +
                "Çözüm: Önce Accounting.Api projesini derleyin (Build > Build Solution)." ,
                apiPath);
        }

        Log.Information("[ApiProcessManager] API başlatılıyor — Path={Path}" , apiPath);

        ProcessStartInfo startInfo = new()
        {
            FileName = apiPath,
            WorkingDirectory = Path.GetDirectoryName(apiPath)!,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        startInfo.Environment["DOTNET_ENVIRONMENT"] = "Development";

        _apiProcess = new Process { StartInfo = startInfo };

        _apiProcess.OutputDataReceived += OnApiOutput;
        _apiProcess.ErrorDataReceived += OnApiError;

        if (!_apiProcess.Start())
        {
            throw new InvalidOperationException("API process başlatılamadı.");
        }

        _apiProcess.BeginOutputReadLine();
        _apiProcess.BeginErrorReadLine();

        Log.Information("[ApiProcessManager] API başlatıldı — PID={Pid}" , _apiProcess.Id);

        bool healthy = await WaitForApiReadyAsync(cancellationToken);

        if (!healthy)
        {
            if (_apiProcess.HasExited)
            {
                throw new InvalidOperationException(
                    $"API process başladıktan kısa süre sonra sonlandı (ExitCode={_apiProcess.ExitCode}).\n\n" +
                    "Muhtemel sebep: Port 5233 başka bir process tarafından tutuluyor olabilir.\n\n" +
                    "Çözüm: taskkill /F /IM Accounting.Api.exe\n\n" +
                    "Detay için log dosyasına bakın:\n" +
                    @"src\Presentation\Accounting.Api\logs\accounting-*.log");
            }

            throw new TimeoutException(
                $"API {HealthCheckTimeoutSeconds} saniye içinde hazır olmadı.\n\n" +
                $"Adres: {HealthCheckUrl}\n\n" +
                "Detay için log dosyasına bakın:\n" +
                @"src\Presentation\Accounting.Api\logs\accounting-*.log");
        }

        Log.Information("[ApiProcessManager] API hazır — {Url}" , ApiBaseUrl);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_apiProcess is { HasExited: false })
        {
            try
            {
                Log.Information("[ApiProcessManager] API process sonlandırılıyor — PID={Pid}" , _apiProcess.Id);
                _apiProcess.Kill(entireProcessTree: true);
                _apiProcess.WaitForExit(3000);
            }
            catch (Exception ex)
            {
                Log.Warning(ex , "[ApiProcessManager] API process sonlandırılamadı.");
            }
        }

        _apiProcess?.Dispose();
        _apiProcess = null;
        _disposed = true;

        GC.SuppressFinalize(this);
    }
    #endregion Operations

    #region Helpers
    private static async Task<bool> IsApiHealthyAsync(CancellationToken cancellationToken)
    {
        try
        {
            using HttpClient client = new() { Timeout = TimeSpan.FromSeconds(2) };
            HttpResponseMessage response = await client.GetAsync(HealthCheckUrl , cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsPortInUse()
    {
        try
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            var listeners = ipProperties.GetActiveTcpListeners();

            foreach (var endpoint in listeners)
            {
                if (endpoint.Port == ApiPort)
                {
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            Log.Warning(ex , "[ApiProcessManager] Port kontrolü başarısız oldu, güvenli taraf olarak 'kullanımda değil' varsayılıyor.");
            return false;
        }
    }

    private static async Task<bool> WaitForApiReadyAsync(CancellationToken cancellationToken)
    {
        using HttpClient client = new() { Timeout = TimeSpan.FromSeconds(2) };

        DateTime deadline = DateTime.UtcNow.AddSeconds(HealthCheckTimeoutSeconds);

        while (DateTime.UtcNow < deadline)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            try
            {
                HttpResponseMessage response = await client.GetAsync(HealthCheckUrl , cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch
            {
                // API henüz hazır değil, bekle
            }

            await Task.Delay(HealthCheckIntervalMs , cancellationToken);
        }

        return false;
    }

    private static string ResolveApiExecutablePath()
    {
        string currentDir = AppContext.BaseDirectory;

        DirectoryInfo? solutionRoot = FindSolutionRoot(currentDir);

        if (solutionRoot is null)
        {
            throw new DirectoryNotFoundException(
                "Solution kökü bulunamadı. Beklenen: MuhasebeErp.slnx dosyasının bulunduğu dizin.");
        }

        string[] candidates =
        [
            Path.Combine(solutionRoot.FullName , DebugPath),
            Path.Combine(solutionRoot.FullName , DebugPathFallback),
            Path.Combine(solutionRoot.FullName , ReleasePath)
        ];

        foreach (string candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return Path.Combine(solutionRoot.FullName , DebugPathFallback);
    }

    private static DirectoryInfo? FindSolutionRoot(string startDirectory)
    {
        DirectoryInfo? dir = new(startDirectory);

        while (dir is not null)
        {
            if (dir.GetFiles("MuhasebeErp.slnx").Length > 0)
            {
                return dir;
            }

            dir = dir.Parent;
        }

        return null;
    }

    private static void OnApiOutput(object sender , DataReceivedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.Data))
        {
            Log.Debug("[API-OUT] {Data}" , e.Data);
        }
    }

    private static void OnApiError(object sender , DataReceivedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.Data))
        {
            Log.Warning("[API-ERR] {Data}" , e.Data);
        }
    }
    #endregion Helpers
}