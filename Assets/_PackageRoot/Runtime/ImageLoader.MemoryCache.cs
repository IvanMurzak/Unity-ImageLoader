using System.Collections.Generic;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
        internal static Dictionary<string, Sprite> memorySpriteCache = new Dictionary<string, Sprite>();

        /// <summary>
        /// Check if the Memory cache contains sprite for the given url
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns true if Sprite exists in Memory cache</returns>
        public static bool MemoryCacheExists(string url)
        {
            return memorySpriteCache.ContainsKey(url);
        }
        /// <summary>
        /// Save sprite to Memory cache directly. Should be used for overloading cache system
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="sprite">sprite which should be saved</param>
        /// <param name="replace">replace existed cached sprite if any</param>
        public static void SaveToMemoryCache(string url, Sprite sprite, bool replace = false)
        {
            if (!settings.useMemoryCache) return;
            if (!replace && memorySpriteCache.ContainsKey(url))
            {
                if (settings.debugLevel <= DebugLevel.Warning)
                    Debug.LogError($"[ImageLoader] Memory cache already contains key: {url}");
                return;
            }
            memorySpriteCache[url] = sprite;
        }
        /// <summary>
        /// Loads directly from Memory cache if exists and allowed
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns null if not allowed to use Memory cache or if there is no cached Sprite</returns>
        public static Sprite LoadFromMemoryCache(string url)
        {
            if (!settings.useMemoryCache) return null;
            return memorySpriteCache.GetValueOrDefault(url);
        }
        /// <summary>
        /// Clear Memory cache for the given url
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        public static void ClearMemoryCache(string url)
        {
            var cache = memorySpriteCache.GetValueOrDefault(url);
            if (cache?.texture != null)
                UnityEngine.Object.DestroyImmediate(cache.texture);

            memorySpriteCache.Remove(url);
        }
        /// <summary>
        /// Clear Memory cache for all urls
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
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