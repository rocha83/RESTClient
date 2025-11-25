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
        private readonly SemaphoreSlim _queueSignal = new SemaphoreSlim(0);
        private static readonly RESTClient<T> _restClient = new RESTClient<T>();
        private static readonly ConcurrentQueue<ResilienceSet<T>> _callQueue = new ConcurrentQueue<ResilienceSet<T>>();

        public ResilienceManager(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ = ListenResilienceQueue();
        }

        private async Task ListenResilienceQueue()
        {
            _logger.LogInformation("Monitoring resiliency queue...");

            while (true)
            {
                await _queueSignal.WaitAsync(); // aguarda item na fila

                if (_callQueue.TryPeek(out var queueItem))
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

        public void Enqueue(ResilienceSet<T> item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _callQueue.Enqueue(item);
            _queueSignal.Release(); // sinaliza que há item na fila
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
                    SendToResilience(resilienceSet);
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

            if (resilienceSet.RetriesDelay > 0)
                await Task.Delay(resilienceSet.RetriesDelay);

            return result;
        }

        private async Task<bool> CallByMethod(ResilienceSet<T> resilienceSet)
        {
            bool result = false;

            if (resilienceSet.CallMethod == HttpMethod.Post)
                result = await _restClient.Post(resilienceSet.ServiceRoute,
                                                resilienceSet.PayLoad,
                                                resilienceSet.CallHeaders,
                                                resilienceSet.CallTimeout);
            else if (resilienceSet.CallMethod == HttpMethod.Put)
                result = await _restClient.Put(resilienceSet.ServiceRoute,
                                               resilienceSet.PayLoad,
                                               resilienceSet.CallHeaders,
                                               resilienceSet.CallTimeout);
            else if (resilienceSet.CallMethod == HttpMethod.Patch)
                result = await _restClient.Patch(resilienceSet.ServiceRoute,
                                                 resilienceSet.PayLoad,
                                                 resilienceSet.CallHeaders,
                                                 resilienceSet.CallTimeout);
            else if (resilienceSet.CallMethod == HttpMethod.Delete)
                result = await _restClient.Delete(resilienceSet.ServiceRoute,
                                                  resilienceSet.PayLoad.ToString(),
                                                  resilienceSet.CallHeaders,
                                                  resilienceSet.CallTimeout);

            return result;
        }

        private void SendToResilience(ResilienceSet<T> resilienceSet)
        {
            _logger.LogInformation($"Sending call for {resilienceSet.ServiceRoute} to resilience queue...");
            resilienceSet.FirstCall = false;
            Enqueue(resilienceSet);
        }

        public void Dispose()
        {
            _queueSignal.Dispose();
            GC.ReRegisterForFinalize(this);
        }
    }
}
