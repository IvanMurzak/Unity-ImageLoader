using System.Collections.Generic;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
        internal static Dictionary<string, Sprite> memorySpriteCache = new Dictionary<string, Sprite>();

        public static bool MemoryCacheExists(string url)
        {
            return memorySpriteCache.ContainsKey(url);
        }
        public static void SaveMemory(string url, Sprite sprite)
        {
            if (!settings.useMemoryCache) return;
            if (memorySpriteCache.ContainsKey(url))
            {
                if (settings.debugMode <= DebugMode.Error)
                    Debug.LogError($"[ImageLoader] Memory cache already contains key: {url}");
                return;
            }
            memorySpriteCache[url] = sprite;
        }
        public static Sprite LoadMemory(string url)
        {
            if (!settings.useMemoryCache) return null;
            return memorySpriteCache.GetValueOrDefault(url);
        }
        public static void ClearMemoryCache(string url)
        {
            var cache = memorySpriteCache.GetValueOrDefault(url);
            if (cache?.texture != null)
                UnityEngine.Object.DestroyImmediate(cache.texture);

            memorySpriteCache.Remove(url);
        }
        public static void ClearMemoryCache()
        {
            foreach (var cache in memorySpriteCache.Values)
            {
                if (cache?.texture != null)
                    UnityEngine.Object.DestroyImmediate(cache.texture);
            }
            memorySpriteCache.Clear();
        }
    }
}