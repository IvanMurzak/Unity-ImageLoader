using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public class Reference<T> : IDisposable
    {
        private static Dictionary<string, int> referenceCounters = new Dictionary<string, int>();
        internal static void Clear()
        {
            lock (referenceCounters) referenceCounters.Clear();
        }
        internal static void Clear(string url)
        {
            lock (referenceCounters) referenceCounters.Remove(url);
        }
        public static int Counter(string url)
        {
            lock (referenceCounters) return referenceCounters.GetValueOrDefault(url);
        }

        /// <summary>
        /// True: Keep the texture in memory, you are responsible to release the memory.
        /// False: Release memory automatically when the Reference.Dispose executed.
        /// </summary>
        public bool Keep { get; set; }
        public T Value { get; private set; }
        public readonly string Url;

        private bool disposed;

        internal Reference(string url, T value)
        {
            Url = url;
            Value = value;

            lock (referenceCounters)
            {
                referenceCounters[url] = referenceCounters.GetValueOrDefault(url, 0) + 1;
                if (ImageLoader.settings.debugLevel <= DebugLevel.Log)
                    Debug.Log($"[ImageLoader] Reference created [{referenceCounters[url]}] URL={url}");
            }
        }

        public Reference<T> SetKeep(bool value = true)
        {
            Keep = value;
            return this;
        }
        public void Dispose()
        {
            if (disposed)
                return;

            Value = default;
            disposed = true;

            lock (referenceCounters)
            {
                if (referenceCounters.GetValueOrDefault(Url) <= 0)
                {
                    if (ImageLoader.settings.debugLevel <= DebugLevel.Warning)
                        Debug.LogError($"[ImageLoader] Reference dispose, Can't dispose URL={Url}");
                    return;
                }

                referenceCounters[Url]--;

                if (Keep)
                {
                    if (ImageLoader.settings.debugLevel <= DebugLevel.Log)
                        Debug.Log($"[ImageLoader] Reference dispose of URL={Url} Ignored. Because 'Keep' is True. Please make sure you release the memory in time to avoid usage of too much memory.");
                    return;
                }

                if (ImageLoader.settings.debugLevel <= DebugLevel.Log)
                    Debug.Log($"[ImageLoader] Reference dispose of URL={Url}");

                if (referenceCounters[Url] == 0)
                    ImageLoader.ClearMemoryCache(Url);
            }
        }
    }
}
