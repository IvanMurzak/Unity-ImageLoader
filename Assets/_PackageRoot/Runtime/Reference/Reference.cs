using System;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public partial class Reference<T> : IDisposable
    {
        private static volatile uint idCounter = 0;

        /// <summary>
        /// True: Keep the texture in memory, you are responsible to release the memory.
        /// False: Release memory automatically when the Reference.Dispose executed.
        /// </summary>
        public bool Keep { get; set; }
        public T Value { get; private set; }
        public readonly string Url;

        private bool disposed;
        public uint Id { get; } = idCounter++;

        internal Reference(string url, T value)
        {
            Url = url;
            Value = value;

            EventOnClearUrl += OnClearUrl;
            EventOnClearAll += OnClearAll;

            lock (referenceCounters)
            {
                referenceCounters.AddOrUpdate(url, 1, (key, oldValue) => oldValue + 1);
                if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Trace))
                    Debug.Log($"[ImageLoader] Ref[id={Id}] Reference created. Total {referenceCounters[url]} references to the object\n{Url}");
            }
        }
        private void OnClearUrl(string url)
        {
            if (Url == url)
                Dispose();
        }
        private void OnClearAll() => Dispose();

        /// <summary>
        /// Set 'Keep' property to True or False
        /// True: Keep the texture in memory, you are responsible to release the memory.
        /// False: Release memory automatically when the Reference.Dispose executed.
        /// </summary>
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

            Value = default;
            disposed = true;

            lock (referenceCounters)
            {
                referenceCounters.AddOrUpdate(Url, 0, (key, oldValue) => oldValue - 1);

                if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Error) && referenceCounters.GetValueOrDefault(Url) < 0)
                    Debug.LogError($"[ImageLoader] Ref[id={Id}] Reference disposed. Total {referenceCounters[Url]} references to the object. CAN'T BE NEGATIVE!\n{Url}");

                if (Keep)
                {
                    if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Trace))
                        Debug.Log($"[ImageLoader] Ref[id={Id}] Reference disposed. Total {referenceCounters[Url]} references to the object. Counter change ignored. Because 'Keep' is True. Please make sure you release the memory in time to avoid usage of too much memory\n{Url}");
                    return;
                }

                if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Trace))
                    Debug.Log($"[ImageLoader] Ref[id={Id}] Reference disposed. Total {referenceCounters[Url]} references to the object\n{Url}");

                if (referenceCounters.GetValueOrDefault(Url) == 0)
                    ImageLoader.ClearMemoryCache(Url);
            }
        }
        ~Reference() => Dispose();
    }
}
