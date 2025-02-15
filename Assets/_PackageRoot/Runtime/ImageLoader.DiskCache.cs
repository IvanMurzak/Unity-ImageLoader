using System.Threading.Tasks;

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