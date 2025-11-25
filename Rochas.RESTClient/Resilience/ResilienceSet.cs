using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Rochas.Net.Connectivity
{
    internal class ResilienceSet<T>
    {
        public ResilienceSet()
        {
        }
        public ResilienceSet(string route, HttpMethod method, int timeout, short retries, 
                             int retriesDelay, IDictionary<string, string>? headers = null, T payLoad = default!)
        {
            PayLoad = payLoad;

            ServiceRoute = route;
            CallMethod = method;
            CallTimeout = timeout;
            CallHeaders = headers;
            CallRetries = retries;
            RetriesDelay = retriesDelay;

            FirstCall = true;
        }

        public string ServiceRoute { get; set; }

        public T PayLoad { get; set; }
        
        public HttpMethod CallMethod { get; set; }

        public int CallTimeout { get; set; }

        public IDictionary<string, string>? CallHeaders { get; set; }

        public short CallRetries { get; set; }

        public int RetriesDelay { get; set; }

        public string LastError { get; set; }

        public bool FirstCall { get; set; }
    }
}
