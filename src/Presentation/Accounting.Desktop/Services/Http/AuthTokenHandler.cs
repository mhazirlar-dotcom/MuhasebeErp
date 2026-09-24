using Accounting.Core.Business.Interfaces.CrossCutting;
using Accounting.Desktop.Interfaces;
using Accounting.Shared.Results;
using Serilog;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Accounting.Desktop.Services.Http;

public sealed class AuthTokenHandler(ITokenStore tokenStore , ITenantContext tenantContext , IAuthTokenRefresher authTokenRefresher) : DelegatingHandler
{
    #region Constants
    private const string HeaderCompanyId = "X-Company-Id";
    private const string HeaderPeriodId = "X-Period-Id";
    #endregion Constants

    #region Operations
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request , CancellationToken cancellationToken)
    {
        Log.Information("[AuthTokenHandler] SendAsync — StoreHash={StoreHash}, HasToken={HasToken}, Method={Method}, Uri={Uri}" , tokenStore.GetHashCode() , tokenStore.HasToken , request.Method , request.RequestUri);

        ApplyAuthorization(request);
        ApplyTenantHeaders(request);

        HttpResponseMessage response = await base.SendAsync(request , cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        Log.Warning("[AuthTokenHandler] 401 alındı — refresh denenecek. Uri={Uri}" , request.RequestUri);

        return await RetryWithRefreshedTokenAsync(request , response , cancellationToken);
    }
    #endregion Operations

    #region Helpers
    private void ApplyAuthorization(HttpRequestMessage request)
    {
        if (!tokenStore.HasToken)
        {
            Log.Warning("[AuthTokenHandler] HasToken=false — Authorization header EKLENMEDİ.");
            return;
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer" , tokenStore.Current!.AccessToken);
        Log.Information("[AuthTokenHandler] Authorization header eklendi.");
    }

    private void ApplyTenantHeaders(HttpRequestMessage request)
    {
        if (tenantContext.HasCompany)
        {
            request.Headers.Add(HeaderCompanyId , tenantContext.CurrentCompanyId.ToString());
        }

        if (tenantContext.HasPeriod)
        {
            request.Headers.Add(HeaderPeriodId , tenantContext.CurrentPeriodId.ToString());
        }
    }

    private async Task<HttpResponseMessage> RetryWithRefreshedTokenAsync(HttpRequestMessage originalRequest , HttpResponseMessage originalResponse , CancellationToken cancellationToken)
    {
        bool canRetry = originalRequest.Headers.Authorization is not null && tokenStore.HasToken;

        if (!canRetry)
        {
            Log.Warning("[AuthTokenHandler] Retry yapılamaz — orijinal istekte Authorization yok veya token store boş.");
            return originalResponse;
        }

        Result<bool> refreshResult = await authTokenRefresher.TryRefreshAsync(cancellationToken);

        if (refreshResult.IsFailure)
        {
            Log.Error("[AuthTokenHandler] Refresh başarısız — Message={Message}. Logout tetikleniyor." , refreshResult.Message);

            originalResponse.Dispose();
            tokenStore.Clear();

            return new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                RequestMessage = originalRequest ,
                ReasonPhrase = "Token yenileme başarısız."
            };
        }

        HttpRequestMessage retryRequest = await CloneRequestAsync(originalRequest , cancellationToken);
        retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer" , tokenStore.Current!.AccessToken);

        ApplyTenantHeaders(retryRequest);

        Log.Information("[AuthTokenHandler] Yeni token ile retry ediliyor — Uri={Uri}" , retryRequest.RequestUri);

        originalResponse.Dispose();

        return await base.SendAsync(retryRequest , cancellationToken);
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage originalRequest , CancellationToken cancellationToken)
    {
        HttpRequestMessage clone = new(originalRequest.Method , originalRequest.RequestUri)
        {
            Version = originalRequest.Version,
            VersionPolicy = originalRequest.VersionPolicy
        };

        foreach (KeyValuePair<string , IEnumerable<string>> header in originalRequest.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key , header.Value);
        }

        if (originalRequest.Content is not null)
        {
            byte[] contentBytes = await originalRequest.Content.ReadAsByteArrayAsync(cancellationToken);

            clone.Content = new ByteArrayContent(contentBytes);

            foreach (KeyValuePair<string , IEnumerable<string>> header in originalRequest.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key , header.Value);
            }
        }

        return clone;
    }
    #endregion Helpers
}