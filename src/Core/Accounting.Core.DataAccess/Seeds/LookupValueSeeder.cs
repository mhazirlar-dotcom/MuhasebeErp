using Accounting.Core.Business.Interfaces.Services;
using Accounting.Core.DataAccess.Persistence.Master;
using Accounting.Core.Domain.Constants;
using Accounting.Core.Domain.Entities;
using Accounting.Shared.Markers;
using Accounting.Shared.Results;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;

namespace Accounting.Core.DataAccess.Seeds;

public class LookupValueSeeder(MasterDbContext context) : IStartupSeeder, IScopedService
{
    #region Constants
    private static readonly (string FileName, string Type)[] SeedFiles =
    [
        ("countries.json", LookupTypes.Country),
        ("cities.json", LookupTypes.City),
        ("districts.json", LookupTypes.District),
        ("trade-registry-offices.json", LookupTypes.TradeRegistryOffice),
        ("professional-organizations.json", LookupTypes.ProfessionalOrganization),
        ("tax-offices.json", LookupTypes.TaxOffice),
        ("address-types.json", LookupTypes.AddressType),
        ("contact-types.json", LookupTypes.ContactType),
        ("legal-statuses.json", LookupTypes.LegalStatus),
        ("legal-natures.json", LookupTypes.LegalNature),
        ("social-security-institutions.json", LookupTypes.SocialSecurityInstitution),
        ("withholding-declaration-methods.json", LookupTypes.WithholdingDeclarationMethod),
        ("create-302-records.json", LookupTypes.Create302Record),
        ("activity-codes.json", LookupTypes.ActivityCode),
        ("accounting-methods.json", LookupTypes.AccountingMethod),
        ("currencies.json", LookupTypes.Currency),
        ("declaration-types.json", LookupTypes.DeclarationType),
        ("exchange-rate-modes.json", LookupTypes.ExchangeRateMode),
        ("ledger-types.json", LookupTypes.LedgerType),
        ("month-names.json", LookupTypes.MonthName),
        ("vat-rates.json", LookupTypes.VatRate),
        ("voucher-number-lengths.json", LookupTypes.VoucherNumberLength),
        ("voucher-sort-modes.json", LookupTypes.VoucherSortMode)
    ];

    private static readonly Dictionary<string, string> HierarchyMap = new()
    {
        { LookupTypes.City , LookupTypes.Country },
        { LookupTypes.District , LookupTypes.City }
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly CultureInfo TurkishCulture = new("tr-TR");
    #endregion Constants

    #region Operations
    public async Task<Result> SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Dictionary<string, Guid> codeIndex = await LoadExistingCodeIndexAsync(cancellationToken);
            HashSet<string> existingKeys = [.. codeIndex.Keys];

            List<LookupValue> toInsert = [];

            foreach ((string fileName , string type) in SeedFiles)
            {
                List<LookupValueSeedDto> dtos = await LoadSeedFileAsync(fileName, cancellationToken);
                if (dtos.Count == 0)
                {
                    continue;
                }

                string parentType = HierarchyMap.TryGetValue(type, out string? pt) ? pt : string.Empty;

                for (int i = 0 ; i < dtos.Count ; i++)
                {
                    LookupValueSeedDto dto = dtos[i];
                    string code = ResolveCode(dto);
                    string key = BuildKey(type, code);

                    if (existingKeys.Contains(key))
                    {
                        continue;
                    }

                    Guid? parentId = null;

                    if (parentType.Length > 0 && !string.IsNullOrWhiteSpace(dto.ParentCode))
                    {
                        string parentKey = BuildKey(parentType, dto.ParentCode);

                        if (codeIndex.TryGetValue(parentKey , out Guid resolvedParentId))
                        {
                            parentId = resolvedParentId;
                        }
                    }

                    LookupValue entity = new()
                    {
                        Type = type,
                        Code = code,
                        Name = dto.Name,
                        ParentId = parentId,
                        DisplayOrder = i,
                        IsActive = true
                    };

                    toInsert.Add(entity);
                    existingKeys.Add(key);
                    codeIndex[key] = entity.Id;
                }
            }

            if (toInsert.Count == 0)
            {
                return Result.Success("Seed verisi zaten güncel.");
            }

            await context.Set<LookupValue>().AddRangeAsync(toInsert , cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Success($"{toInsert.Count} seed kaydı eklendi.");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Seed hatası: {ex.Message}" , ResultStatus.InternalError);
        }
    }
    #endregion Operations

    #region Helpers
    private async Task<Dictionary<string , Guid>> LoadExistingCodeIndexAsync(CancellationToken cancellationToken)
    {
        var rows = await context.Set<LookupValue>()
            .AsNoTracking()
            .Select(x => new { Key = x.Type + "|" + x.Code , x.Id })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(x => x.Key , x => x.Id);
    }

    private static async Task<List<LookupValueSeedDto>> LoadSeedFileAsync(
        string fileName ,
        CancellationToken cancellationToken)
    {
        string assemblyName = typeof(LookupValueSeeder).Assembly.GetName().Name!;
        string resourceName = $"{assemblyName}.Seeds.Data.{fileName}";

        using Stream stream = typeof(LookupValueSeeder).Assembly
            .GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Seed dosyası bulunamadı: {resourceName}");

        List<LookupValueSeedDto>? dtos = await JsonSerializer.DeserializeAsync<List<LookupValueSeedDto>>(
            stream,
            JsonOptions,
            cancellationToken);

        return dtos ?? [];
    }

    private static string ResolveCode(LookupValueSeedDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Code))
        {
            return dto.Code;
        }

        if (!string.IsNullOrWhiteSpace(dto.ParentCode))
        {
            return $"{dto.ParentCode}-{BuildCodeFromName(dto.Name)}";
        }

        throw new InvalidOperationException($"'{dto.Name}' için Code üretilemiyor.");
    }

    private static string BuildCodeFromName(string name)
    {
        string upperCased = name.ToUpper(TurkishCulture);
        return upperCased.Replace(' ' , '-');
    }

    private static string BuildKey(string type , string code)
    {
        return $"{type}|{code}";
    }
    #endregion Helpers
}