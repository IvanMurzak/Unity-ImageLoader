﻿using System;
using System.Collections.Generic;

namespace Extensions.Unity.ImageLoader
{
    public partial class Reference<T> : IDisposable
    {
        private volatile static Dictionary<string, int> referenceCounters = new Dictionary<string, int>();
        internal static void Clear()
        {
            lock (referenceCounters) referenceCounters.Clear();
            EventOnClearAll?.Invoke();
        }
        internal static bool Clear(string url)
        {
            var result = false;
            lock (referenceCounters) result = referenceCounters.Remove(url);
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
