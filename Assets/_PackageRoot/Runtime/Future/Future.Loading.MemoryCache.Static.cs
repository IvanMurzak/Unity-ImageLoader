using System;
using System.Collections.Generic;
using Extensions.Unity.ImageLoader.Utils;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public abstract partial class Future<T>
    {
        internal static volatile Dictionary<string, T> memoryCache = new Dictionary<string, T>();

        // internal static void ClearMemoryCache()
        // {
        //     // Support for turning off domain reload in Project Settings/Editor/Enter Play Mode Settings
        //     // Sprites created with Sprite.Create gets destroyed when exiting play mode, so we need to clear the sprite cache, as otherwise the cache will be
        //     // filled with destroyed sprites when the user reenters play mode.
        //     lock (memoryCache) memoryCache.Clear();
        //         Reference<T>.Clear();
        // }

        /// <summary>
        /// Check the Memory cache contains sprite for the given url
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns true if Sprite exists in Memory cache</returns>
        public static bool MemoryCacheContains(string url)
        {
            lock (memoryCache)
                return memoryCache.ContainsKey(url);
        }
        /// <summary>
        /// Save sprite to Memory cache directly. Should be used for overloading cache system
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="obj">sprite which should be saved</param>
        /// <param name="replace">replace existed cached sprite if any</param>
        public static void SaveToMemoryCache(string url, T obj, bool replace = false, bool suppressMessage = false)
        {
            lock (memoryCache)
            {
                if (!replace && memoryCache.ContainsKey(url))
                {
                    if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Warning))
                        Debug.LogError($"[ImageLoader] Can't set to Memory cache ({typeof(T).Name}), because it already contains the key. Use 'replace = true' to replace\n{url}");
                    return;
                }
                if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Trace) && !suppressMessage)
                    Debug.Log($"[ImageLoader] Save to Memory cache ({typeof(T).Name})\n{url}");
                memoryCache[url] = obj;
            }
        }
        /// <summary>
        /// Loads directly from Memory cache if exists and allowed
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns null if not allowed to use Memory cache or if there is no cached Sprite</returns>
        public static Reference<T> LoadFromMemoryCacheRef(string url)
        {
            T obj;

            lock (memoryCache)
                obj = memoryCache.GetValueOrDefault(url);

            if (obj == null)
                return null;

            return new Reference<T>(url, obj);
        }
        /// <summary>
        /// Loads directly from Memory cache if exists and allowed
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns null if not allowed to use Memory cache or if there is no cached Sprite</returns>
        public static T LoadFromMemoryCache(string url)
        {
            lock (memoryCache)
                return memoryCache.GetValueOrDefault(url);
        }
        /// <summary>
        /// Clear Memory cache for the given url
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        public static void ClearMemoryCache(string url, Action<T> releaseMemory)
        {
            if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Log))
                Debug.Log($"[ImageLoader] Clearing Memory cache ({typeof(T).Name})\n{url}");

            var refCount = Reference<T>.Counter(url);
            if (refCount > 0)
                throw new Exception($"[ImageLoader] There are {refCount} references to the sprite, clear them first. URL={url}");

            lock (memoryCache)
            {
                if (memoryCache.Remove(url, out var cache))
                {
                    releaseMemory?.Invoke(cache);
                }
            }
        }
        /// <summary>
        /// Clear Memory cache for all urls
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        public static void ClearMemoryCacheAll(Action<T> releaseMemory)
        {
            if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Log))
                Debug.Log($"[ImageLoader] Clearing Memory cache ({typeof(T).Name}) All");

            lock (memoryCache)
            {
                var toKeep = new List<KeyValuePair<string, T>>();
                foreach (var keyValue in memoryCache)
                {
                    var url = keyValue.Key;
                    var refCount = Reference<T>.Counter(url);
                    if (refCount > 0)
                    {
                        if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Error))
                            Debug.LogError($"[ImageLoader] There are {refCount} references to the object, clear them first. URL={url}");
                        toKeep.Add(keyValue);
                        continue;
                    }

                    var cache = keyValue.Value;
                    Safe.Run(releaseMemory, cache, DebugLevel.Exception);
                }
                memoryCache.Clear();

                // Restoring not released references
                if (toKeep.Count > 0)
                {
                    foreach (var keyValue in toKeep)
                        memoryCache[keyValue.Key] = keyValue.Value;
                }
            }
        }
    }
}
