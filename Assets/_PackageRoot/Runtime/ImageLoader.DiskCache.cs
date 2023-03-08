using System.IO;
using System.Threading.Tasks;

namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
        internal static readonly TaskFactory diskTaskFactory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(1));
        private static string DiskCachePath(string url) => $"{settings.diskSaveLocation}/I_{url.GetHashCode()}";
        private static void SaveDisk(string url, byte[] data)
        {
            if (!settings.useDiskCache) return;
            Directory.CreateDirectory(settings.diskSaveLocation);
            Directory.CreateDirectory(Path.GetDirectoryName(DiskCachePath(url)));
            File.WriteAllBytes(DiskCachePath(url), data);
        }
        private static byte[] LoadDisk(string url)
        {
            if (!settings.useDiskCache) return null;
            Directory.CreateDirectory(settings.diskSaveLocation);
            Directory.CreateDirectory(Path.GetDirectoryName(DiskCachePath(url)));
            if (!DiskCacheExists(url)) return null;
            return File.ReadAllBytes(DiskCachePath(url));
        }
        private static Task SaveDiskAsync(string url, byte[] data)
        {
            if (!settings.useDiskCache)
                return Task.CompletedTask;

            return diskTaskFactory.StartNew(() => SaveDisk(url, data));
        }
        private static Task<byte[]> LoadDiskAsync(string url)
        {
            if (!settings.useDiskCache)
                return Task.FromResult<byte[]>(null);

            return diskTaskFactory.StartNew(() => LoadDisk(url));
        }

        public static bool DiskCacheExists(string url) => File.Exists(DiskCachePath(url));
        public static void ClearDiskCache()
        {
            if (Directory.Exists(settings.diskSaveLocation))
                Directory.Delete(settings.diskSaveLocation, true);
        }
    }
}