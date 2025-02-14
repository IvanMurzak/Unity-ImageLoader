using System;
using System.Collections.Generic;
using Extensions.Unity.ImageLoader.Utils;

namespace Extensions.Unity.ImageLoader
{
    public partial class Reference<T> : IDisposable
    {
        private volatile static Dictionary<string, int> referenceCounters = new Dictionary<string, int>();
        internal static void Clear()
        {
            lock (referenceCounters) referenceCounters.Clear();
            Safe.Run(EventOnClearAll, DebugLevel.Exception);
        }
        internal static bool Clear(string url)
        {
            var result = false;
            lock (referenceCounters) result = referenceCounters.Remove(url);
            Safe.Run(EventOnClearUrl, url, DebugLevel.Exception);
            return result;
        }
        public static int Counter(string url)
        {
            lock (referenceCounters) return referenceCounters.GetValueOrDefault(url);
        }

        private static WeakAction<string> EventOnClearUrl = new WeakAction<string>();
        private static WeakAction EventOnClearAll = new WeakAction();
    }
}
