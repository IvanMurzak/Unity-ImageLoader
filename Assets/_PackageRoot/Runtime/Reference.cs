using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public class Reference : IDisposable
    {
        private static Dictionary<string, int> referenceCounters = new Dictionary<string, int>();

        private string url;
        private bool disposed;
        
        public Sprite Sprite { get; internal set; }

        internal Reference(string url)
        {
            this.url = url;
            lock (referenceCounters)
            {
                referenceCounters[url] = referenceCounters.GetValueOrDefault(url, 0) + 1;
            }
        }
        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            lock (referenceCounters)
            {
                if (referenceCounters.GetValueOrDefault(url) <= 0)
                {
                    if (ImageLoader.settings.debugLevel <= DebugLevel.Warning)
                        UnityEngine.Debug.LogError($"Can't dispose URL={url}");
                    return;
                }

                referenceCounters[url]--;

                if (referenceCounters[url] == 0)
                    ImageLoader.ClearMemoryCache(url);
            }
        }
    }
}
