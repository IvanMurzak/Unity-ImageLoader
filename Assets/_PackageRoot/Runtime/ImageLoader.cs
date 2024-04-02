using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
        private static HashSet<string> loadingInProcess = new HashSet<string>();
        private static void AddLoading(string url) => loadingInProcess.Add(url);
        private static void RemoveLoading(string url) => loadingInProcess.Remove(url);

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
        public static bool IsLoading(string url) => loadingInProcess.Contains(url);

        /// <summary>
        /// Clear cache from Memory and Disk layers for all urls
        /// </summary>
        public static Task ClearCache()
        {
            ClearMemoryCache();
            return ClearDiskCache();
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