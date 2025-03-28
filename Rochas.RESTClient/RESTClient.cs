﻿using System;
using System.Web;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

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

        private HttpResponseMessage? response;

        #endregion

        #region Constructors

        public RESTClient() {}

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
            using var restCall = new HttpClient();
            if (!string.IsNullOrWhiteSpace(serviceRoute))
            {
                ConfigureHeadersAndTimeout(restCall, headers, timeout);

                response = await restCall.GetAsync(serviceRoute);

                return await response.Content.ReadFromJsonAsync<T>();
            }
            else
                throw new ArgumentException(emptySvcRouteMsg);
        }

        public async Task<T> Get(string serviceRoute, string id, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            using (var restCall = new HttpClient())
            {
                if (!string.IsNullOrWhiteSpace(serviceRoute))
                {
                    ConfigureHeadersAndTimeout(restCall, headers, timeout);

                    var serviceRouteId = string.Format(urlFormat, serviceRoute, id);
                    response = await restCall.GetAsync(serviceRouteId);

                    return await response.Content.ReadFromJsonAsync<T>();
                }
                else
                    throw new ArgumentException(emptySvcRouteMsg);
            }
        }

        public async Task<T> GetWithParams(string serviceRoute, string parameters, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            using (var restCall = new HttpClient())
            {
                if (!string.IsNullOrWhiteSpace(serviceRoute))
                {
                    ConfigureHeadersAndTimeout(restCall, headers, timeout);

                    var encodedParams = EncodeParameterValues(parameters);
                    var serviceRouteParam = string.Format(urlParamFormat, serviceRoute, encodedParams);
                    response = await restCall.GetAsync(serviceRouteParam);

                    return await response.Content.ReadFromJsonAsync<T>();
                }
                else
                    throw new InvalidOperationException(emptySvcRouteMsg);
            }
        }

        public async Task<bool> Post(string serviceRoute, T payLoad, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            using var restCall = new HttpClient();
            if (!string.IsNullOrWhiteSpace(serviceRoute))
            {
                ConfigureHeadersAndTimeout(restCall, headers, timeout);

                if (!UseResilience)
                    response = await restCall.PostAsJsonAsync(serviceRoute, payLoad);
                else
                {
                    var resilienceSet = new ResilienceSet<T>(serviceRoute, HttpMethod.Post, timeout, 
                                                             _callRetries, _retriesDelay, headers, payLoad);
                    return await _resilienceManager.TryCall(resilienceSet);
                }

                return response.IsSuccessStatusCode;
            }

            throw new InvalidOperationException(emptySvcRouteMsg);
        }

        public async Task<R> PostWithResponse<R>(string serviceRoute, T payLoad, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            using var restCall = new HttpClient();
            if (!string.IsNullOrWhiteSpace(serviceRoute))
            {
                ConfigureHeadersAndTimeout(restCall, headers, timeout);

                response = await restCall.PostAsJsonAsync(serviceRoute, payLoad);

                return await response.Content.ReadFromJsonAsync<R>();
            }

            throw new InvalidOperationException(emptySvcRouteMsg);
        }

        public async Task<bool> Put(string serviceRoute, T payLoad, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            using var restCall = new HttpClient();
            if (!string.IsNullOrWhiteSpace(serviceRoute))
            {
                ConfigureHeadersAndTimeout(restCall, headers, timeout);

                if (!UseResilience)
                    response = await restCall.PutAsJsonAsync(serviceRoute, payLoad);
                else
                {
                    var resilienceSet = new ResilienceSet<T>(serviceRoute, HttpMethod.Put, timeout, 
                                                             _callRetries, _retriesDelay, headers, payLoad);
                    return await _resilienceManager.TryCall(resilienceSet);
                }

                return response.IsSuccessStatusCode;
            }
            throw new InvalidOperationException(emptySvcRouteMsg);
        }

        public async Task<R> PutWithResponse<R>(string serviceRoute, T payLoad, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            using var restCall = new HttpClient();
            if (!string.IsNullOrWhiteSpace(serviceRoute))
            {
                ConfigureHeadersAndTimeout(restCall, headers, timeout);

                response = await restCall.PutAsJsonAsync(serviceRoute, payLoad);

                return await response.Content.ReadFromJsonAsync<R>();
            }

            throw new InvalidOperationException(emptySvcRouteMsg);
        }

        public async Task<bool> Patch(string serviceRoute, T payLoad, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            using var restCall = new HttpClient();
            if (!string.IsNullOrWhiteSpace(serviceRoute))
            {
                ConfigureHeadersAndTimeout(restCall, headers, timeout);

                if (!UseResilience)
                    response = await restCall.PatchAsJsonAsync(serviceRoute, payLoad);
                else
                {
                    var resilienceSet = new ResilienceSet<T>(serviceRoute, HttpMethod.Patch, timeout, 
                                                             _callRetries, _retriesDelay, headers, payLoad);
                    return await _resilienceManager.TryCall(resilienceSet);
                }

                return response.IsSuccessStatusCode;
            }
            throw new InvalidOperationException(emptySvcRouteMsg);
        }

        public async Task<R> PatchWithResponse<R>(string serviceRoute, T payLoad, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            using var restCall = new HttpClient();
            if (!string.IsNullOrWhiteSpace(serviceRoute))
            {
                ConfigureHeadersAndTimeout(restCall, headers, timeout);

                response = await restCall.PatchAsJsonAsync(serviceRoute, payLoad);

                return await response.Content.ReadFromJsonAsync<R>();
            }

            throw new InvalidOperationException(emptySvcRouteMsg);
        }

        public async Task<bool> Delete(string serviceRoute, string id, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            using var restCall = new HttpClient();
            if (!string.IsNullOrWhiteSpace(serviceRoute))
            {
                ConfigureHeadersAndTimeout(restCall, headers, timeout);

                var serviceRouteId = string.Format(urlFormat, serviceRoute, id);
                if (!UseResilience)
                    response = await restCall.DeleteAsync(serviceRouteId);
                else
                {
                    var resilienceSet = new ResilienceSet<T>(serviceRoute, HttpMethod.Delete, timeout, _callRetries, _retriesDelay);
                    return await _resilienceManager.TryCall(resilienceSet);
                }

                return response.IsSuccessStatusCode;
            }
            throw new InvalidOperationException(emptySvcRouteMsg);
        }

        public async Task<bool> DeleteWithParams(string serviceRoute, string parameters, 
                                                 IDictionary<string, string>? headers = null, int timeout = 0)
        {
            using var restCall = new HttpClient();
            if (!string.IsNullOrWhiteSpace(serviceRoute))
            {
                ConfigureHeadersAndTimeout(restCall, headers, timeout);

                var serviceRouteId = string.Format(urlParamFormat, serviceRoute, parameters);
                if (!UseResilience)
                    response = await restCall.DeleteAsync(serviceRouteId);
                else
                {
                    var resilienceSet = new ResilienceSet<T>(serviceRoute, HttpMethod.Delete, timeout, _callRetries, _retriesDelay);
                    return await _resilienceManager.TryCall(resilienceSet);
                }

                return response.IsSuccessStatusCode;
            }
            throw new InvalidOperationException(emptySvcRouteMsg);
        }

        #endregion

        #region Public Sync Methods

        public T GetSync(string serviceRoute, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            return Get(serviceRoute, headers, timeout).Result;
        }

        public T GetSync(string serviceRoute, string id, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            return Get(serviceRoute, id, headers, timeout).Result;
        }

        public T GetWithParamsSync(string serviceRoute, string parameters, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            return GetWithParams(serviceRoute, parameters, headers, timeout).Result;
        }

        public bool PostSync(string serviceRoute, T payLoad, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            return Post(serviceRoute, payLoad, headers,timeout).Result;
        }

        public R PostWithResponseSync<R>(string serviceRoute, T payLoad, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            return PostWithResponse<R>(serviceRoute, payLoad, headers, timeout).Result;
        }

        public bool PutSync(string serviceRoute, T payLoad, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            return Put(serviceRoute, payLoad, headers, timeout).Result;
        }

        public R PutWithResponseSync<R>(string serviceRoute, T payLoad, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            return PutWithResponse<R>(serviceRoute, payLoad, headers, timeout).Result;
        }

        public bool PatchSync(string serviceRoute, T payLoad, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            return Patch(serviceRoute, payLoad, headers, timeout).Result;
        }

        public R PatchWithResponseSync<R>(string serviceRoute, T payLoad, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            return PatchWithResponse<R>(serviceRoute, payLoad, headers, timeout).Result;
        }

        public bool DeleteSync(string serviceRoute, string id, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            return Delete(serviceRoute, id, headers, timeout).Result;
        }

        public bool DeleteWithParamsSync(string serviceRoute, string parameters, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            return DeleteWithParams(serviceRoute, parameters, headers, timeout).Result;
        }

        #endregion

        #region Helper Methods

        private static void ConfigureHeadersAndTimeout(HttpClient restCall, IDictionary<string, string>? headers = null, int timeout = 0)
        {
            if (headers != null)
                foreach (var header in headers)
                    restCall.DefaultRequestHeaders.Add(header.Key, header.Value);

            if (timeout > 0)
                restCall.Timeout = TimeSpan.FromSeconds(timeout);
        }

        private static string? EncodeParameterValues(string parameters)
        {
            StringBuilder preResult = new StringBuilder();
            var arrParams = parameters.Split('&');

            foreach (var param in arrParams)
            {
                var arrParamItem = param.Split('=');

                // Two-pass encode for special chars
                if (!arrParamItem[1].Contains("%"))
                    arrParamItem[1] = HttpUtility.UrlEncode(arrParamItem[1]);

                if (arrParamItem[1].Contains("+") || arrParamItem[1].Contains("/"))
                    arrParamItem[1] = arrParamItem[1].Replace("+", "%2b").Replace("/", "2f");

                preResult.Append(String.Join("=", arrParamItem));
                preResult.Append("&");
            }

            var result = preResult.ToString();

            if (result.Length > 1)
            {
                if (result.EndsWith("==&"))
                    return string.Concat(result.Substring(0, (result.Length - 3)), HttpUtility.UrlEncode("=="));
                else if (result.EndsWith("=&"))
                    return string.Concat(result.Substring(0, (result.Length - 2)), HttpUtility.UrlEncode("="));
                else
                    return result.Substring(0, (result.Length - 1));
            }
            else
                return null;
        }

        public void Dispose()
        {
            if (_resilienceManager != null)
                _resilienceManager.Dispose();

            GC.ReRegisterForFinalize(this);
        }

        #endregion
    }
}
