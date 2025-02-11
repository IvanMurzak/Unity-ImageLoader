using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Extensions.Unity.ImageLoader
{
    public partial class Reference<T> : IDisposable
    {
        private volatile static ConcurrentDictionary<string, int> referenceCounters = new ConcurrentDictionary<string, int>();
        internal static void Clear()
        {
            lock (referenceCounters) referenceCounters.Clear();
            EventOnClearAll?.Invoke();
        }
        internal static bool Clear(string url)
        {
            var result = false;
            lock (referenceCounters) result = referenceCounters.Remove(url, out var _);
            EventOnClearUrl?.Invoke(url);
            return result;
        }
        public static int Counter(string url)
        {
            lock (referenceCounters) return referenceCounters.GetValueOrDefault(url);
        }

        private delegate void ClearUrlHandler(string url);
        private delegate void ClearAllHandler();

        private static event ClearUrlHandler EventOnClearUrl;
        private static event ClearAllHandler EventOnClearAll;
    }
}
