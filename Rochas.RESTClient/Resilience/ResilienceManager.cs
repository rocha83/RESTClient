using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Rochas.Net.Connectivity
{
    internal class ResilienceManager<T> : IDisposable
    {
        private readonly ILogger _logger;
        private static ConcurrentQueue<ResilienceSet<T>> _callQueue
                         = new ConcurrentQueue<ResilienceSet<T>>();

        public ResilienceManager(ILogger logger)
        {
            _logger = logger;

            var resilienceThread = new Thread(async () => await ListenResilienceQueue());
            resilienceThread.Start();
        }

        private async Task ListenResilienceQueue()
        {
            _logger.LogInformation($"Monitoring resiliency queue...");

            while (true)
            {
                if (!_callQueue.IsEmpty)
                {
                    if (_callQueue.TryPeek(out var queueItem))
                    {
                        if (queueItem.CallRetries > 0)
                        {
                            _logger.LogInformation($"Trying to make a call to {queueItem.ServiceRoute}...");

                            queueItem.CallRetries--;
                            var result = await TryCall(queueItem);
                            if (result)
                                queueItem.CallRetries = 0;
                        }
                        else
                            _callQueue.TryDequeue(out var queueDeadItem);
                    }
                }
            }
        }

        public async Task<bool> TryCall(ResilienceSet<T> resilienceSet)
        {
            bool result = false;

            try
            {
                result = await CallByMethod(resilienceSet);
                if (result)
                    _logger.LogInformation($"Call to {resilienceSet.ServiceRoute} made successfully.");
                else if (resilienceSet.FirstCall)
                {
                    SendToResilience(resilienceSet);
                }

                if (resilienceSet.RetriesDelay > 0)
                    Thread.Sleep(resilienceSet.RetriesDelay);
            }
            catch (Exception ex)
            {
                resilienceSet.LastError = ex.Message;
                if (resilienceSet.FirstCall)
                {
                    _logger.LogWarning($"Problem making a call to {resilienceSet.ServiceRoute}.");
                    SendToResilience(resilienceSet);
                }
                else if (resilienceSet.CallRetries == 0)
                    _logger.LogError($"Error making the call to {resilienceSet.ServiceRoute}: {Environment.NewLine} {ex.Message}");
            }

            return result;
        }

        private async Task<bool> CallByMethod(ResilienceSet<T> resilienceSet)
        {
            var result = false;
            using (var client = new RESTClient<T>())
            {
                if (resilienceSet.CallMethod == HttpMethod.Post)
                    result = await client.Post(resilienceSet.ServiceRoute,
                                               resilienceSet.PayLoad,
                                               resilienceSet.CallTimeout);
                else if (resilienceSet.CallMethod == HttpMethod.Put)
                    result = await client.Put(resilienceSet.ServiceRoute,
                                              resilienceSet.PayLoad,
                                              resilienceSet.CallTimeout);
                else if (resilienceSet.CallMethod == HttpMethod.Patch)
                    result = await client.Patch(resilienceSet.ServiceRoute,
                                                resilienceSet.PayLoad, 
                                                resilienceSet.CallTimeout);
                else if (resilienceSet.CallMethod == HttpMethod.Delete)
                    result = await client.Patch(resilienceSet.ServiceRoute,
                                                resilienceSet.PayLoad, 
                                                resilienceSet.CallTimeout);
                return result;
            }
        }

        private void SendToResilience(ResilienceSet<T> resilienceSet)
        {
            _logger.LogInformation($"Sending call for {resilienceSet.ServiceRoute} to resilience queue...");

            resilienceSet.FirstCall = false;
            _callQueue.Enqueue(resilienceSet);
        }

        public void Dispose()
        {
            GC.ReRegisterForFinalize(this);
        }
    }
}