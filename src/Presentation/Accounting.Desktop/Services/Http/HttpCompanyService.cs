using Accounting.Core.Business.Interfaces.Services;
using Accounting.Core.Domain.Entities;
using Accounting.Desktop.Extensions;
using Accounting.Shared.Dtos.Master;
using Accounting.Shared.Markers;
using Accounting.Shared.Results;
using System.Net.Http;
using System.Net.Http.Json;

namespace Accounting.Desktop.Services.Http;

public sealed class HttpCompanyService(IHttpClientFactory httpClientFactory) : ICompanyService, IApiClient
{
    #region Fields
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
    #endregion Fields

    #region Operations
    public async Task<Result<IReadOnlyList<Company>>> GetByUserIdAsync(bool includeActive = true , bool includePassive = false , CancellationToken cancellationToken = default)
    {
        string url = $"api/companies?includeActive={includeActive}&includePassive={includePassive}";
        HttpResponseMessage response = await _httpClient.GetAsync(url , cancellationToken);

        return await response.ReadResultAsync<IReadOnlyList<Company>>(cancellationToken);
    }

    public async Task<Result<Company>> GetByIdAsync(Guid id , CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"api/companies/{id}" , cancellationToken);

        return await response.ReadResultAsync<Company>(cancellationToken);
    }

    public async Task<Result<Company>> CreateAsync(Company entity , CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/companies" , entity , cancellationToken);

        return await response.ReadResultAsync<Company>(cancellationToken);
    }

    public async Task<Result> UpdateAsync(Company entity , CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/companies/{entity.Id}" , entity , cancellationToken);

        return await response.ReadResultAsync(cancellationToken);
    }

    public async Task<Result> DeleteAsync(Guid id , CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.DeleteAsync($"api/companies/{id}" , cancellationToken);

        return await response.ReadResultAsync(cancellationToken);
    }

    public async Task<Result> SetActiveAsync(Guid id , bool isActive , CancellationToken cancellationToken = default)
    {
        SetCompanyActiveRequest request = new(isActive);

        HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/companies/{id}/active" , request , cancellationToken);

        return await response.ReadResultAsync(cancellationToken);
    }
    #endregion Operations
}