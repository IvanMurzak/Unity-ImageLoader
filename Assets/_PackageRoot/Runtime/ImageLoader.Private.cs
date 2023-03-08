using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
        private static HashSet<string> loadingInProcess = new HashSet<string>();
        private static Dictionary<string, Sprite> loadedSpritesCache = new Dictionary<string, Sprite>();

        private static readonly TaskFactory factory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(1));
        private static string DiskCachePath(string url) => $"{SaveLocation}/I_{url.GetHashCode()}";
        private static void Save(string url, byte[] data)
        {
            Directory.CreateDirectory(SaveLocation);
            Directory.CreateDirectory(Path.GetDirectoryName(DiskCachePath(url)));
            File.WriteAllBytes(DiskCachePath(url), data);
        }
        private static byte[] Load(string url)
        {
            Directory.CreateDirectory(SaveLocation);
            Directory.CreateDirectory(Path.GetDirectoryName(DiskCachePath(url)));
            if (!DiskCacheExists(url)) return null;
            return File.ReadAllBytes(DiskCachePath(url));
        }
        private static Task SaveAsync(string url, byte[] data) => factory.StartNew(() => Save(url, data));
        private static Task<byte[]> LoadAsync(string url) => factory.StartNew(() => Load(url));
    }
}