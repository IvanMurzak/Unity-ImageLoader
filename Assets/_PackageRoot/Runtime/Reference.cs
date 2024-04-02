using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public partial struct Reference<T> : IDisposable
    {
        public static Reference<T> Empty = new Reference<T>();

        /// <summary>
        /// True: Keep the texture in memory, you are responsible to release the memory.
        /// False: Release memory automatically when the Reference.Dispose executed.
        /// </summary>
        public bool Keep { get; set; }
        public bool IsValid { get; private set; }
        public T Value { get; private set; }

        private bool disposed;
        public readonly string Url;

        internal Reference(string url, T value)
        {
            Url = url;
            Value = value;
            Keep = false;
            disposed = false;
            IsValid = !string.IsNullOrEmpty(url);

            EventOnClearUrl += OnClearUrl;
            EventOnClearAll += OnClearAll;

            lock (referenceCounters)
            {
                referenceCounters[url] = Math.Max(0, referenceCounters.GetValueOrDefault(url, 0)) + 1;
                if (ImageLoader.settings.debugLevel <= DebugLevel.Log)
                    Debug.Log($"[ImageLoader] Reference created [{referenceCounters[url]}] URL={url}");
            }
        }
        private void OnClearUrl(string url)
        {
            if (Url == url)
                Dispose();
        }
        private void OnClearAll()
        {
            Dispose();
        }

        public Reference<T> SetKeep(bool value = true)
        {
            Keep = value;
            return this;
        }
        public void Dispose()
        {
            EventOnClearUrl -= OnClearUrl;
            EventOnClearAll -= OnClearAll;

            if (disposed)
                return;

            IsValid = false;
            Value = default;
            disposed = true;

            lock (referenceCounters)
            {
                if (referenceCounters.ContainsKey(Url))
                    referenceCounters[Url]--;

                if (referenceCounters.GetValueOrDefault(Url) < 0)
                {
                    if (ImageLoader.settings.debugLevel <= DebugLevel.Warning)
                        Debug.LogError($"[ImageLoader] Reference dispose has negative counter URL={Url}");
                }

                if (Keep)
                {
                    if (ImageLoader.settings.debugLevel <= DebugLevel.Log)
                        Debug.Log($"[ImageLoader] Reference dispose of URL={Url} Ignored. Because 'Keep' is True. Please make sure you release the memory in time to avoid usage of too much memory.");
                    return;
                }

                if (ImageLoader.settings.debugLevel <= DebugLevel.Log)
                    Debug.Log($"[ImageLoader] Reference dispose of URL={Url}");

                if (!referenceCounters.ContainsKey(Url) || referenceCounters[Url] == 0)
                    ImageLoader.ClearMemoryCache(Url);
            }
        }
    }
}
