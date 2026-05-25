using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
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
        public static bool IsLoadingSprite<T>(string url) => Future<Sprite>.IsLoading(url);

        /// <summary>
        /// Check if the url is loading right now
        /// </summary>
        /// <returns>Returns true if the url is loading right now</returns>
        public static bool IsLoadingTexture<T>(string url) => Future<Texture2D>.IsLoading(url);

        /// <summary>
        /// Find and return current loading Future by the url
        /// <param name="url">URL to the picture, web or local</param>
        /// </summary>
        /// <returns>Returns current loading Future or null if none</returns>
        public static Future<Sprite> GetLoadingSpriteFuture<T>(string url) => Future<Sprite>.GetLoadingFuture(url);

        /// <summary>
        /// Find and return current loading Future by the url
        /// <param name="url">URL to the picture, web or local</param>
        /// </summary>
        /// <returns>Returns current loading Future or null if none</returns>
        public static Future<Texture2D> GetLoadingTextureFuture<T>(string url) => Future<Texture2D>.GetLoadingFuture(url);

        /// <summary>
        /// Return all current loading Futures
        /// </summary>
        /// <returns>Returns read only list of all current loading Futures</returns>
        public static IReadOnlyCollection<Future<Sprite>> GetLoadingSpriteFutures() => Future<Sprite>.GetLoadingFutures();

        /// <summary>
        /// Return all current loading Futures
        /// </summary>
        /// <returns>Returns read only list of all current loading Futures</returns>
        public static IReadOnlyCollection<Future<Texture2D>> GetLoadingTextureFutures() => Future<Texture2D>.GetLoadingFutures();

        /// <summary>
        /// Clear cache from Memory and Disk layers for all urls
        /// </summary>
        /// <returns>Returns task of the disk cache clearing process</returns>
        public static Task ClearCacheAll()
        {
            ClearMemoryCacheAll();
            return ClearDiskCacheAll();
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
        public static Sprite ToSprite(this Texture2D texture, float pixelDensity = 100f)
            => Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelDensity);

        /// <summary>
        /// Converts Texture2D to Sprite
        /// </summary>
        /// <param name="texture">Texture for creation Sprite</param>
        /// <param name="pivot">Pivot of created Sprite</param>
        /// <param name="pixelDensity">Pixel density of the Sprite</param>
        /// <returns>Returns sprite</returns>
        public static Sprite ToSprite(this Texture2D texture, Vector2 pivot, float pixelDensity = 100f)
            => Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), pivot, pixelDensity);

        /// <summary>
        /// Get reference count of the image
        /// </summary>
        /// <param name="url">URL to the image, web or local</param>
        /// <returns>Returns reference count</returns>
        public static int GetReferenceCount(string url) => Reference<Texture2D>.Counter(url);
    }
}