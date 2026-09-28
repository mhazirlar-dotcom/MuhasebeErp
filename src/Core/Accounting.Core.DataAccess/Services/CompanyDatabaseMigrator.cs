using Accounting.Core.Business.Interfaces.Repositories;
using Accounting.Core.Business.Interfaces.Services;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Markers;
using Accounting.Shared.Results;
using Serilog;
using System.Reflection;
using System.Text.Json;

namespace Accounting.Core.DataAccess.Services;

public sealed class CompanyDatabaseMigrator(
    ICompanyRepository companyRepository ,
    IDatabaseInitializer databaseInitializer) : ICompanyDatabaseMigrator, IScopedService
{
    #region Constants
    private const string ManifestFileName = ".migration-manifest.json";
    private const int ParallelismDegree = 8;
    private static readonly TimeSpan ManifestTtl = TimeSpan.FromHours(1);
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    #endregion Constants

    #region Operations
    public async Task<Result<int>> MigrateAllAsync(CancellationToken cancellationToken = default)
    {
        string manifestPath = ResolveManifestPath();
        string currentFingerprint = ComputeFingerprint();

        MigrationManifest? manifest = ReadManifest(manifestPath);

        if (manifest is not null && IsManifestFresh(manifest , currentFingerprint))
        {
            Log.Information(
                "[CompanyDatabaseMigrator] Migration manifest taze — son çalışma: {LastRun:O}, atlanıyor." ,
                manifest.LastRunUtc);

            return Result<int>.Success(0 , "Migration manifest taze, atlandı.");
        }

        Result<IReadOnlyList<Company>> companiesResult = await companyRepository.GetAllAsync(cancellationToken);

        if (companiesResult.IsFailure)
        {
            return Result<int>.Failure(companiesResult.Message , companiesResult.Status);
        }

        IReadOnlyList<Company> companies = companiesResult.Data;

        if (companies.Count == 0)
        {
            Log.Information("[CompanyDatabaseMigrator] Güncellenecek firma bulunamadı.");
            WriteManifest(manifestPath , currentFingerprint , successCount: 0 , failureCount: 0);
            return Result<int>.Success(0 , "Güncellenecek firma yok.");
        }

        Log.Information(
            "[CompanyDatabaseMigrator] Migration başlıyor — Toplam {Count} firma, eşzamanlılık: {Parallelism}" ,
            companies.Count , ParallelismDegree);

        int successCount = 0;
        int failureCount = 0;

        using SemaphoreSlim throttler = new(ParallelismDegree);

        IEnumerable<Task> tasks = companies.Select(async company =>
        {
            await throttler.WaitAsync(cancellationToken);

            try
            {
                Result migrateResult = await databaseInitializer.InitializeCompanyDatabaseAsync(company , cancellationToken);

                if (migrateResult.IsSuccess)
                {
                    Interlocked.Increment(ref successCount);

                    Log.Information(
                        "[CompanyDatabaseMigrator] Firma DB güncellendi — CompanyId={CompanyId}, ShortName={ShortName}, Database={Database}" ,
                        company.Id , company.ShortName , company.DatabaseName);
                }
                else
                {
                    Interlocked.Increment(ref failureCount);

                    Log.Warning(
                        "[CompanyDatabaseMigrator] Firma DB güncellenemedi — CompanyId={CompanyId}, ShortName={ShortName}, Message={Message}" ,
                        company.Id , company.ShortName , migrateResult.Message);
                }
            }
            finally
            {
                throttler.Release();
            }
        });

        await Task.WhenAll(tasks);

        string summary = $"Toplam {companies.Count} firma — Başarılı: {successCount}, Başarısız: {failureCount}.";

        if (failureCount == 0)
        {
            WriteManifest(manifestPath , currentFingerprint , successCount , failureCount);
        }

        if (failureCount > 0)
        {
            return Result<int>.Failure(summary , ResultStatus.InternalError);
        }

        Log.Information("[CompanyDatabaseMigrator] {Summary}" , summary);

        return Result<int>.Success(successCount , summary);
    }

    public async Task<Result> MigrateOneAsync(Guid companyId , CancellationToken cancellationToken = default)
    {
        if (companyId == Guid.Empty)
        {
            return Result.Failure("Firma Id zorunludur." , ResultStatus.ValidationError);
        }

        Result<Company> companyResult = await companyRepository.GetByIdAsync(companyId , cancellationToken);

        if (companyResult.IsFailure)
        {
            return Result.NotFound(companyResult.Message);
        }

        Company company = companyResult.Data;

        Result migrateResult = await databaseInitializer.InitializeCompanyDatabaseAsync(company , cancellationToken);

        if (migrateResult.IsSuccess)
        {
            Log.Information(
                "[CompanyDatabaseMigrator] Firma DB güncellendi — CompanyId={CompanyId}, ShortName={ShortName}, Database={Database}" ,
                company.Id , company.ShortName , company.DatabaseName);
        }

        return migrateResult;
    }
    #endregion Operations

    #region Helpers
    private static string ResolveManifestPath()
    {
        return Path.Combine(AppContext.BaseDirectory , ManifestFileName);
    }

    private static string ComputeFingerprint()
    {
        Assembly assembly = typeof(CompanyDatabaseMigrator).Assembly;

        string assemblyName = assembly.GetName().Name ?? "unknown";
        string assemblyVersion = assembly.GetName().Version?.ToString() ?? "0.0.0.0";
        long assemblyTicks = File.GetLastWriteTimeUtc(assembly.Location).Ticks;

        return $"{assemblyName}|{assemblyVersion}|{assemblyTicks}";
    }

    private static MigrationManifest? ReadManifest(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return null;
            }

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<MigrationManifest>(json);
        }
        catch (Exception ex)
        {
            Log.Warning(ex , "[CompanyDatabaseMigrator] Manifest okunamadı, sıfırdan çalıştırılacak.");
            return null;
        }
    }

    private static void WriteManifest(string path , string fingerprint , int successCount , int failureCount)
    {
        try
        {
            MigrationManifest manifest = new()
            {
                LastRunUtc = DateTime.UtcNow,
                AssemblyFingerprint = fingerprint,
                SuccessCount = successCount,
                FailureCount = failureCount
            };

            string json = JsonSerializer.Serialize(manifest , JsonOptions);
            File.WriteAllText(path , json);
        }
        catch (Exception ex)
        {
            Log.Warning(ex , "[CompanyDatabaseMigrator] Manifest yazılamadı.");
        }
    }

    private static bool IsManifestFresh(MigrationManifest manifest , string currentFingerprint)
    {
        if (manifest.FailureCount > 0)
        {
            return false;
        }

        if (!string.Equals(manifest.AssemblyFingerprint , currentFingerprint , StringComparison.Ordinal))
        {
            return false;
        }

        TimeSpan age = DateTime.UtcNow - manifest.LastRunUtc;

        return age < ManifestTtl;
    }
    #endregion Helpers

    #region Nested Types
    private sealed class MigrationManifest
    {
        #region Properties
        public DateTime LastRunUtc { get; set; }
        public string AssemblyFingerprint { get; set; } = string.Empty;
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        #endregion Properties
    }
    #endregion Nested Types
}