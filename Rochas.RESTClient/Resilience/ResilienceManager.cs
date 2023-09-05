using System;
using System.Net;
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
            _logger.LogInformation($"Monitorando fila de resiliência...");

            while (true)
            {
                if (ResilienceQueue<T>.HasItems)
                {
                    var queueItem = ResilienceQueue<T>.Dequeue();
                    if (queueItem != null && queueItem.CallRetries > 0)
                    {
                        _logger.LogInformation($"Tentando realizar chamada para {queueItem.ServiceRoute}...");

                        queueItem.CallRetries--;
                        var result = await TryCall(queueItem);
                        if (result)
                            queueItem.CallRetries = 0;
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
                    _logger.LogInformation($"Chamada para {resilienceSet.ServiceRoute} realizada com sucesso.");
                else
                    if (resilienceSet.FirstCall)
                        SendToResilience(resilienceSet);
            }
            catch (Exception ex)
            {
                resilienceSet.LastError = ex.Message;
                if (resilienceSet.FirstCall)
                {
                    _logger.LogWarning($"Problema ao realizar a chamada para {resilienceSet.ServiceRoute}.");
                    SendToResilience(resilienceSet);
                }
                else if (resilienceSet.CallRetries == 0)
                    _logger.LogError($"Erro ao realizar a chamada para {resilienceSet.ServiceRoute}: {Environment.NewLine} {ex.Message}");
            }

            return result;
        }

        private async Task<bool> CallByMethod(ResilienceSet<T> resilienceSet)
        {
            var result = false;
            using (var client = new RESTClient<T>())
            {
                if (resilienceSet.CallMethod == HttpMethod.Post)
                    await client.Post(resilienceSet.ServiceRoute,
                                      resilienceSet.PayLoad);
                else if (resilienceSet.CallMethod == HttpMethod.Put)
                    await client.Put(resilienceSet.ServiceRoute,
                                     resilienceSet.PayLoad);
                else if (resilienceSet.CallMethod == HttpMethod.Patch)
                    await client.Patch(resilienceSet.ServiceRoute,
                                                  resilienceSet.PayLoad);
                else if (resilienceSet.CallMethod == HttpMethod.Delete)
                    await client.Patch(resilienceSet.ServiceRoute,
                                                  resilienceSet.PayLoad);
                return result;
            }
        }

        private void SendToResilience(ResilienceSet<T> resilienceSet)
        {
            _logger.LogInformation($"Enviando chamada para {resilienceSet.ServiceRoute} à fila de resiliência...");

            resilienceSet.FirstCall = false;
            _callQueue.Enqueue(resilienceSet);
        }

        public void Dispose()
        {
            GC.ReRegisterForFinalize(this);
        }
    }
}