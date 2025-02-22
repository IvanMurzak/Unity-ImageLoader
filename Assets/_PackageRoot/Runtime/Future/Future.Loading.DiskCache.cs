using System.Threading.Tasks;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T>
    {
        /// <summary>
        /// Check if the image is cached at Disk
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns true if image is cached at Disk</returns>
        protected virtual bool DiskCacheContains() => DiskCacheContains(Url);
        protected virtual Task SaveDiskAsync(byte[] data)
        {
            if (LogLevel.IsActive(DebugLevel.Log))
                Debug.Log($"[ImageLoader] Save to Disk cache ({typeof(T).Name})\n{Url}");
            return SaveDiskAsync(Url, data, LogLevel);
        }
        protected virtual Task<byte[]> LoadDiskAsync() => LoadDiskAsync(Url, LogLevel);
        protected abstract T ParseBytes(byte[] bytes);
    }
}
