using System.Threading.Tasks;

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
        protected virtual Task SaveDiskAsync(byte[] data) => SaveDiskAsync(Url, data);
        protected virtual Task<byte[]> LoadDiskAsync() => LoadDiskAsync(Url);
        protected abstract T ParseBytes(byte[] bytes);
    }
}
