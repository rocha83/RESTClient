using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Rochas.Net.Connectivity
{
    public class RESTClient<T> : IDisposable
    {
        #region Declarations

        private static readonly string urlFormat = "{0}/{1}";
        private static readonly string urlParamFormat = "{0}?{1}";
        private static readonly string emptySvcRouteMsg = "Invalid service route";

        private readonly ResilienceManager<T> _resilienceManager;
        private readonly short _callRetries;
        private readonly int _retriesDelay;

        private static readonly HttpClient _httpClient = new HttpClient();

        #endregion

        #region Constructors

        public RESTClient() { }

        public RESTClient(ILogger<T> logger, short callRetries, int retriesDelay = 0)
        {
            _callRetries = callRetries;
            _retriesDelay = retriesDelay;
            _resilienceManager = new ResilienceManager<T>(logger);
            UseResilience = true;
        }

        #endregion

        #region Public Properties

        public bool UseResilience { get; set; }

        #endregion

        #region Public Async Methods

        public async Task<T> Get(string serviceRoute, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            if (string.IsNullOrWhiteSpace(serviceRoute))
                throw new ArgumentException(emptySvcRouteMsg);

            ConfigureHeadersAndTimeout(_httpClient, headers, timeout);

            var response = await _httpClient.GetAsync(serviceRoute);
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<T> Get(string serviceRoute, string id, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            if (string.IsNullOrWhiteSpace(serviceRoute))
                throw new ArgumentException(emptySvcRouteMsg);

            var serviceRouteId = string.Format(urlFormat, serviceRoute, id);
            return await Get(serviceRouteId, headers, timeout);
        }

        public async Task<T> GetWithParams(string serviceRoute, IDictionary<string, string> parameters, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            if (string.IsNullOrWhiteSpace(serviceRoute))
                throw new ArgumentException(emptySvcRouteMsg);

            var query = EncodeParameters(parameters);
            var serviceRouteParam = string.Format(urlParamFormat, serviceRoute, query);
            return await Get(serviceRouteParam, headers, timeout);
        }

        public async Task<bool> Post(string serviceRoute, T payLoad, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            if (string.IsNullOrWhiteSpace(serviceRoute))
                throw new InvalidOperationException(emptySvcRouteMsg);

            ConfigureHeadersAndTimeout(_httpClient, headers, timeout);

            if (!UseResilience)
            {
                var response = await _httpClient.PostAsJsonAsync(serviceRoute, payLoad);
                return response.IsSuccessStatusCode;
            }
            else
            {
                var resilienceSet = new ResilienceSet<T>(serviceRoute, HttpMethod.Post, timeout, _callRetries, _retriesDelay, headers, payLoad);
                return await _resilienceManager.TryCall(resilienceSet);
            }
        }

        public async Task<R> PostWithResponse<R>(string serviceRoute, T payLoad, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            if (string.IsNullOrWhiteSpace(serviceRoute))
                throw new InvalidOperationException(emptySvcRouteMsg);

            ConfigureHeadersAndTimeout(_httpClient, headers, timeout);

            var response = await _httpClient.PostAsJsonAsync(serviceRoute, payLoad);
            return await response.Content.ReadFromJsonAsync<R>();
        }

        public async Task<bool> Put(string serviceRoute, T payLoad, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            if (string.IsNullOrWhiteSpace(serviceRoute))
                throw new InvalidOperationException(emptySvcRouteMsg);

            ConfigureHeadersAndTimeout(_httpClient, headers, timeout);

            if (!UseResilience)
            {
                var response = await _httpClient.PutAsJsonAsync(serviceRoute, payLoad);
                return response.IsSuccessStatusCode;
            }
            else
            {
                var resilienceSet = new ResilienceSet<T>(serviceRoute, HttpMethod.Put, timeout, _callRetries, _retriesDelay, headers, payLoad);
                return await _resilienceManager.TryCall(resilienceSet);
            }
        }

        public async Task<R> PutWithResponse<R>(string serviceRoute, T payLoad, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            if (string.IsNullOrWhiteSpace(serviceRoute))
                throw new InvalidOperationException(emptySvcRouteMsg);

            ConfigureHeadersAndTimeout(_httpClient, headers, timeout);

            var response = await _httpClient.PutAsJsonAsync(serviceRoute, payLoad);
            return await response.Content.ReadFromJsonAsync<R>();
        }

        public async Task<bool> Patch(string serviceRoute, T payLoad, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            if (string.IsNullOrWhiteSpace(serviceRoute))
                throw new InvalidOperationException(emptySvcRouteMsg);

            ConfigureHeadersAndTimeout(_httpClient, headers, timeout);

            if (!UseResilience)
            {
                var response = await _httpClient.PatchAsJsonAsync(serviceRoute, payLoad);
                return response.IsSuccessStatusCode;
            }
            else
            {
                var resilienceSet = new ResilienceSet<T>(serviceRoute, HttpMethod.Patch, timeout, _callRetries, _retriesDelay, headers, payLoad);
                return await _resilienceManager.TryCall(resilienceSet);
            }
        }

        public async Task<R> PatchWithResponse<R>(string serviceRoute, T payLoad, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            if (string.IsNullOrWhiteSpace(serviceRoute))
                throw new InvalidOperationException(emptySvcRouteMsg);

            ConfigureHeadersAndTimeout(_httpClient, headers, timeout);

            var response = await _httpClient.PatchAsJsonAsync(serviceRoute, payLoad);
            return await response.Content.ReadFromJsonAsync<R>();
        }

        public async Task<bool> Delete(string serviceRoute, string id, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            if (string.IsNullOrWhiteSpace(serviceRoute))
                throw new InvalidOperationException(emptySvcRouteMsg);

            ConfigureHeadersAndTimeout(_httpClient, headers, timeout);

            var serviceRouteId = string.Format(urlFormat, serviceRoute, id);

            if (!UseResilience)
            {
                var response = await _httpClient.DeleteAsync(serviceRouteId);
                return response.IsSuccessStatusCode;
            }
            else
            {
                var resilienceSet = new ResilienceSet<T>(serviceRoute, HttpMethod.Delete, timeout, _callRetries, _retriesDelay);
                return await _resilienceManager.TryCall(resilienceSet);
            }
        }

        public async Task<bool> DeleteWithParams(string serviceRoute, IDictionary<string, string> parameters,
                                                 IDictionary<string, string>? headers = null, int timeout = 0)
        {
            if (string.IsNullOrWhiteSpace(serviceRoute))
                throw new InvalidOperationException(emptySvcRouteMsg);

            var query = EncodeParameters(parameters);
            var serviceRouteParam = string.Format(urlParamFormat, serviceRoute, query);
            return await Delete(serviceRouteParam, string.Empty, headers, timeout);
        }

        #endregion

        #region Public Sync Methods (Worker-friendly)

        public T GetSync(string serviceRoute, IDictionary<string, string>? headers = null, int timeout = 0)
            => Get(serviceRoute, headers, timeout).GetAwaiter().GetResult();

        public T GetSync(string serviceRoute, string id, IDictionary<string, string>? headers = null, int timeout = 0)
            => Get(serviceRoute, id, headers, timeout).GetAwaiter().GetResult();

        public T GetWithParamsSync(string serviceRoute, IDictionary<string, string> parameters,
                                   IDictionary<string, string>? headers = null, int timeout = 0)
            => GetWithParams(serviceRoute, parameters, headers, timeout).GetAwaiter().GetResult();

        public bool PostSync(string serviceRoute, T payLoad, IDictionary<string, string>? headers = null, int timeout = 0)
            => Post(serviceRoute, payLoad, headers, timeout).GetAwaiter().GetResult();

        public R PostWithResponseSync<R>(string serviceRoute, T payLoad, IDictionary<string, string>? headers = null, int timeout = 0)
            => PostWithResponse<R>(serviceRoute, payLoad, headers, timeout).GetAwaiter().GetResult();

        public bool PutSync(string serviceRoute, T payLoad, IDictionary<string, string>? headers = null, int timeout = 0)
            => Put(serviceRoute, payLoad, headers, timeout).GetAwaiter().GetResult();

        public R PutWithResponseSync<R>(string serviceRoute, T payLoad, IDictionary<string, string>? headers = null, int timeout = 0)
            => PutWithResponse<R>(serviceRoute, payLoad, headers, timeout).GetAwaiter().GetResult();

        public bool PatchSync(string serviceRoute, T payLoad, IDictionary<string, string>? headers = null, int timeout = 0)
            => Patch(serviceRoute, payLoad, headers, timeout).GetAwaiter().GetResult();

        public R PatchWithResponseSync<R>(string serviceRoute, T payLoad, IDictionary<string, string>? headers = null, int timeout = 0)
            => PatchWithResponse<R>(serviceRoute, payLoad, headers, timeout).GetAwaiter().GetResult();

        public bool DeleteSync(string serviceRoute, string id, IDictionary<string, string>? headers = null, int timeout = 0)
            => Delete(serviceRoute, id, headers, timeout).GetAwaiter().GetResult();

        public bool DeleteWithParamsSync(string serviceRoute, IDictionary<string, string> parameters,
                                         IDictionary<string, string>? headers = null, int timeout = 0)
            => DeleteWithParams(serviceRoute, parameters, headers, timeout).GetAwaiter().GetResult();

        #endregion

        #region Helper Methods

        private static void ConfigureHeadersAndTimeout(HttpClient restCall, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            restCall.DefaultRequestHeaders.Clear();

            if (headers != null)
                foreach (var header in headers)
                    restCall.DefaultRequestHeaders.Add(header.Key, header.Value);

            if (timeout > 0)
                restCall.Timeout = TimeSpan.FromSeconds(timeout);
        }

        private static string EncodeParameters(IDictionary<string, string> parameters)
        {
            var query = new List<string>();
            foreach (var param in parameters)
            {
                var encodedKey = Uri.EscapeDataString(param.Key);
                var encodedValue = Uri.EscapeDataString(param.Value);
                query.Add($"{encodedKey}={encodedValue}");
            }
            return string.Join("&", query);
        }

        public void Dispose()
        {
            _resilienceManager?.Dispose();
        }

        #endregion
    }
}
