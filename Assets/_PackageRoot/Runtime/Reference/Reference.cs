using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public partial class Reference<T> : IDisposable
    {
        private static int idCounter = 0;

        /// <summary>
        /// True: Keep the texture in memory, you are responsible to release the memory.
        /// False: Release memory automatically when the Reference.Dispose executed.
        /// </summary>
        public bool Keep { get; set; }
        public T Value { get; private set; }
        public readonly string Url;

        private volatile bool disposed;
        internal readonly int id = idCounter++;

        internal Reference(string url, T value)
        {
            Url = url;
            Value = value;

            // WARNING: It is a reference from outside to this object.
            // The object will not be disposed automatically. Explicit call Dispose() required.
            // TODO: To avoid the reference to let the object be disposed automatically.
            EventOnClearUrl += OnClearUrl;
            EventOnClearAll += OnClearAll;

            lock (referenceCounters)
            {
                referenceCounters.AddOrUpdate(url, 1, (key, oldValue) => oldValue + 1);
                if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Log))
                    Debug.Log($"[ImageLoader] Ref[id={id}] Reference created [{referenceCounters[url]}] URL={url}");
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

            Value = default;
            disposed = true;

            lock (referenceCounters)
            {
                referenceCounters.AddOrUpdate(Url, 0, (key, oldValue) => oldValue - 1);

                if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Error) && referenceCounters.GetValueOrDefault(Url) < 0)
                    Debug.LogError($"[ImageLoader] Ref[id={id}] Reference dispose [{referenceCounters[Url]}] has negative counter URL={Url}");

                if (Keep)
                {
                    if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Log))
                        Debug.Log($"[ImageLoader] Ref[id={id}] Reference dispose [{referenceCounters[Url]}] URL={Url} Ignored. Because 'Keep' is True. Please make sure you release the memory in time to avoid usage of too much memory.");
                    return;
                }

                if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Log))
                    Debug.Log($"[ImageLoader] Ref[id={id}] Reference dispose [{referenceCounters[Url]}] URL={Url}");

                if (referenceCounters.GetValueOrDefault(Url) == 0)
                    ImageLoader.ClearMemoryCache(Url);
            }
        }
        ~Reference() => Dispose();
    }
}
