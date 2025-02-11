using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;

namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
        private static ConcurrentDictionary<string, Future<Sprite>> loadingInProcess = new ConcurrentDictionary<string, Future<Sprite>>();
        private static void AddLoading(Future<Sprite> future)
        {
            if (!loadingInProcess.TryAdd(future.Url, future))
                throw new Exception($"[ImageLoader] Future[id={future.id}] AddLoading: {future.Url} already loading");

            if (settings.debugLevel <= DebugLevel.Log)
                Debug.Log($"[ImageLoader] Future[id={future.id}] AddLoading: {future.Url}, total {loadingInProcess.Count} loading tasks");
        }
        private static void RemoveLoading(Future<Sprite> future) => RemoveLoading(future.Url);
        private static void RemoveLoading(string url)
        {
            if (loadingInProcess.TryRemove(url, out var future))
            {
                if (settings.debugLevel <= DebugLevel.Log)
                    Debug.Log($"[ImageLoader] Future[id={future.id}] RemoveLoading: {url}, left {loadingInProcess.Count} loading tasks");
            }
            else
            {
                if (settings.debugLevel <= DebugLevel.Warning)
                    Debug.LogWarning($"[ImageLoader] Future[id={future.id}] RemoveLoading: {url} not found in loading tasks");
            }
        }

        /// <summary>
        /// Initialization of static variables, should be called from main thread at project start
        /// </summary>
        public static void Init()
        {
            // need get SaveLocation variable in runtime from thread to setup the default static value into it
            var temp = settings.diskSaveLocation + settings.diskSaveLocation;
        }

        /// <summary>
        /// Check if the url is loading right now
        /// </summary>
        /// <returns>Returns true if the url is loading right now</returns>
        public static bool IsLoading(string url) => loadingInProcess.ContainsKey(url);

        /// <summary>
        /// Find and return current loading Future by the url
        /// <param name="url">URL to the picture, web or local</param>
        /// </summary>
        /// <returns>Returns current loading Future or null if none</returns>
        public static Future<Sprite> GetLoadingFuture(string url) => loadingInProcess.TryGetValue(url, out var future) ? future : null;

        /// <summary>
        /// Return all current loading Futures
        /// </summary>
        /// <returns>Returns read only list of all current loading Futures</returns>
        public static IReadOnlyCollection<Future<Sprite>> GetLoadingFutures() => loadingInProcess.Values.ToArray();

        /// <summary>
        /// Clear cache from Memory and Disk layers for all urls
        /// </summary>
        /// <returns>Returns task of the disk cache clearing process</returns>
        public static Task ClearCache()
        {
            ClearMemoryCache();
            return ClearDiskCache();
        }

        /// <summary>
        /// Clear cache from Memory and Disk layers for all urls
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns task of the disk cache clearing process</returns>
        public static Task ClearCache(string url)
        {
            ClearMemoryCache(url);
            return ClearDiskCache(url);
        }

        /// <summary>
        /// Checks cache at Memory and at Disk by the url
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns true if cache exists</returns>
        public static bool CacheContains(string url) => MemoryCacheContains(url) || DiskCacheContains(url);

        /// <summary>
        /// Converts Texture2D to Sprite
        /// </summary>
        /// <param name="texture">Texture for creation Sprite</param>
        /// <param name="pixelDensity">Pixel density of the Sprite</param>
        /// <returns>Returns sprite</returns>
        public static Sprite ToSprite(Texture2D texture, float pixelDensity = 100f)
            => Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelDensity);

        /// <summary>
        /// Converts Texture2D to Sprite
        /// </summary>
        /// <param name="texture">Texture for creation Sprite</param>
        /// <param name="pivot">Pivot of created Sprite</param>
        /// <param name="pixelDensity">Pixel density of the Sprite</param>
        /// <returns>Returns sprite</returns>
        public static Sprite ToSprite(Texture2D texture, Vector2 pivot, float pixelDensity = 100f)
            => Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), pivot, pixelDensity);
    }
}