using System;
using System.Collections.Concurrent;

namespace Rochas.Net.Connectivity
{
    internal static class ResilienceQueue<T>
    {
        private static readonly ConcurrentQueue<ResilienceSet<T>> queue 
            = new ConcurrentQueue<ResilienceSet<T>>();

        public static void Enqueue(ResilienceSet<T> resilienceSet)
        {
            if (resilienceSet != null)
                queue?.Enqueue(resilienceSet);
        }

        public static ResilienceSet<T>? Dequeue()
        {
            ResilienceSet<T>? queueItem = null;

            if (!queue.IsEmpty)
            {
                queue?.TryPeek(out queueItem);
                if ((queueItem != null) && (queueItem.CallRetries == 0))
                    queue?.TryDequeue(out queueItem);
            }

            return queueItem;
        }

        public static bool HasItems
        {
            get { return !queue.IsEmpty; }
        }
    }
}
