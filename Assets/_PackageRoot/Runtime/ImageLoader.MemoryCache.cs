using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
        internal static volatile Dictionary<string, Sprite> memorySpriteCache = new Dictionary<string, Sprite>();

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void ClearMemoryCacheOnEnterPlayMode()
        {
            // Support for turning off domain reload in Project Settings/Editor/Enter Play Mode Settings
            // Sprites created with Sprite.Create gets destroyed when exiting play mode, so we need to clear the sprite cache, as otherwise the cache will be
            // filled with destroyed sprites when the user reenters play mode.
            memorySpriteCache.Clear();
            Reference<Sprite>.Clear();
        }
#endif

        /// <summary>
        /// Check the Memory cache contains sprite for the given url
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns true if Sprite exists in Memory cache</returns>
        public static bool MemoryCacheContains(string url)
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
            if (settings.debugLevel <= DebugLevel.Log)
                Debug.Log($"[ImageLoader] Save to memory cache: {url}");
            memorySpriteCache[url] = sprite;
        }
        /// <summary>
        /// Loads directly from Memory cache if exists and allowed
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns null if not allowed to use Memory cache or if there is no cached Sprite</returns>
        public static Reference<Sprite>? LoadFromMemoryCacheRef(string url)
        {
            if (!settings.useMemoryCache) return null;

            var sprite = memorySpriteCache.GetValueOrDefault(url);
            if (sprite == null)
                return null;

            return new Reference<Sprite>(url, sprite);
        }
        /// <summary>
        /// Loads directly from Memory cache if exists and allowed
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns null if not allowed to use Memory cache or if there is no cached Sprite</returns>
        public static Sprite? LoadFromMemoryCache(string url)
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
            if (settings.debugLevel <= DebugLevel.Log)
                Debug.Log($"[ImageLoader] Clearing Memory cache: {url}");
            Reference<Sprite>.Clear(url);
            if (memorySpriteCache.Remove(url, out var cache))
            {
                if (!ReferenceEquals(cache, null) && cache != null &&
                    !ReferenceEquals(cache.texture, null))
                    UnityEngine.Object.DestroyImmediate(cache.texture);
            }
        }
        /// <summary>
        /// Clear Memory cache for all urls
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        public static void ClearMemoryCache()
        {
            if (settings.debugLevel <= DebugLevel.Log)
                Debug.Log($"[ImageLoader] Clearing Memory cache All");
            Reference<Sprite>.Clear();
            foreach (var cache in memorySpriteCache.Values)
            {
                if (!ReferenceEquals(cache, null) && cache != null &&
                    !ReferenceEquals(cache.texture, null))
                    UnityEngine.Object.DestroyImmediate(cache.texture);
            }
            memorySpriteCache.Clear();
        }
    }
}
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.