using System.Threading.Tasks;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
        /// <summary>
        /// Check if the image is cached at Disk
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns true if image is cached at Disk</returns>
        public static bool DiskCacheContains(string url) => FutureTexture.DiskCacheContains(url);

        /// <summary>
        /// Check if the image is cached at Disk
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns true if image is cached at Disk</returns>
        public static Task<bool> DiskCacheExistsAsync(string url) => FutureTexture.DiskCacheExistsAsync(url);

        /// <summary>
        /// Save sprite to Disk cache directly. Should be used for overloading cache system
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="sprite">sprite which should be saved</param>
        /// <param name="replace">replace existed cached sprite if any</param>
        public static Task SaveToDiskCache(string url, Sprite sprite, bool replace = false, DebugLevel logLevel = DebugLevel.Error)
            => SaveToDiskCache(url, sprite.texture.EncodeToPNG(), replace, logLevel);

        /// <summary>
        /// Save sprite to Disk cache directly. Should be used for overloading cache system
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="texture">sprite which should be saved</param>
        /// <param name="replace">replace existed cached sprite if any</param>
        public static Task SaveToDiskCache(string url, Texture2D texture, bool replace = false, DebugLevel logLevel = DebugLevel.Error)
            => SaveToDiskCache(url, texture.EncodeToPNG(), replace, logLevel);

        /// <summary>
        /// Save sprite to Disk cache directly. Should be used for overloading cache system
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="data">encoded texture which should be saved</param>
        /// <param name="replace">replace existed cached sprite if any</param>
        public static Task SaveToDiskCache(string url, byte[] data, bool replace = false, DebugLevel logLevel = DebugLevel.Error)
            => FutureTexture.SaveToDiskCache(url, data, replace, logLevel);


        /// <summary>
        /// Clear Disk cache for all urls
        /// </summary>
        public static Task ClearDiskCacheAll() => FutureTexture.ClearDiskCache();

        /// <summary>
        /// Clear Disk cache for the given url
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        public static Task ClearDiskCache(string url) => FutureTexture.ClearDiskCache(url);
    }
}