using Accounting.Core.Business.Interfaces.Services;
using Accounting.Core.Domain.Entities;
using Accounting.Desktop.Extensions;
using Accounting.Shared.Markers;
using Accounting.Shared.Results;
using System.Net.Http;
using System.Net.Http.Json;

namespace Accounting.Desktop.Services.Http;

public sealed class HttpPeriodService(IHttpClientFactory httpClientFactory) : IPeriodService, IApiClient
{
    #region Fields
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
    #endregion Fields

    #region Operations
    public async Task<Result<IReadOnlyList<Period>>> GetByCompanyIdAsync(Guid companyId , CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"api/periods?companyId={companyId}" , cancellationToken);

        return await response.ReadResultAsync<IReadOnlyList<Period>>(cancellationToken);
    }

    public async Task<Result<Period>> GetByIdAsync(Guid id , CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"api/periods/{id}" , cancellationToken);

        return await response.ReadResultAsync<Period>(cancellationToken);
    }

    public async Task<Result<Period>> CreateAsync(Period entity , CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/periods" , entity , cancellationToken);

        return await response.ReadResultAsync<Period>(cancellationToken);
    }

    public async Task<Result> UpdateAsync(Period entity , CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/periods/{entity.Id}" , entity , cancellationToken);

        return await response.ReadResultAsync(cancellationToken);
    }

    public async Task<Result> DeleteAsync(Guid id , CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.DeleteAsync($"api/periods/{id}" , cancellationToken);

        return await response.ReadResultAsync(cancellationToken);
    }
    #endregion Operations
}