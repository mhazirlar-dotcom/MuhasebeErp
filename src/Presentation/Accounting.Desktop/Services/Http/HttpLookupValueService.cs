using Accounting.Core.Business.Interfaces.Services;
using Accounting.Core.Domain.Entities;
using Accounting.Desktop.Extensions;
using Accounting.Shared.Markers;
using Accounting.Shared.Results;
using System.Net.Http;

namespace Accounting.Desktop.Services.Http;

public sealed class HttpLookupValueService(IHttpClientFactory httpClientFactory) : ILookupValueService, IApiClient
{
    #region Constants
    private const string CrudNotSupportedMessage = "Lookup CRUD işlemi HTTP üzerinden henüz desteklenmiyor.";
    #endregion Constants

    #region Fields
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
    #endregion Fields

    #region Read Operations
    public async Task<Result<IReadOnlyList<LookupValue>>> GetByTypeAsync(string type , bool includeInactive = false , CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"api/lookups/{type}?includeInactive={includeInactive}" , cancellationToken);

        return await response.ReadResultAsync<IReadOnlyList<LookupValue>>(cancellationToken);
    }

    public async Task<Result<LookupValue>> GetByTypeAndCodeAsync(string type , string code , CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"api/lookups/{type}/{code}" , cancellationToken);

        return await response.ReadResultAsync<LookupValue>(cancellationToken);
    }

    public async Task<Result<IReadOnlyList<LookupValue>>> GetChildrenAsync(Guid parentId , bool includeInactive = false , CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"api/lookups/children/{parentId}?includeInactive={includeInactive}" , cancellationToken);

        return await response.ReadResultAsync<IReadOnlyList<LookupValue>>(cancellationToken);
    }
    #endregion Read Operations

    #region Crud Operations (Lookup Admin Ekranı gelene kadar stub)
    public Task<Result<LookupValue>> GetByIdAsync(Guid id , CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<LookupValue>.Failure(CrudNotSupportedMessage , ResultStatus.InternalError));
    }

    public Task<Result<LookupValue>> CreateAsync(LookupValue entity , CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<LookupValue>.Failure(CrudNotSupportedMessage , ResultStatus.InternalError));
    }

    public Task<Result> UpdateAsync(LookupValue entity , CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Failure(CrudNotSupportedMessage , ResultStatus.InternalError));
    }

    public Task<Result> DeleteAsync(Guid id , CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Failure(CrudNotSupportedMessage , ResultStatus.InternalError));
    }
    #endregion Crud Operations
}