﻿using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T>
    {
        internal static readonly TaskFactory diskTaskFactory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(1));

        protected static string DiskCacheFolderPath => $"{ImageLoader.settings.diskSaveLocation}/_{typeof(T).Name}";
        protected static string DiskCachePath(string url) => $"{DiskCacheFolderPath}/_{url.GetHashCode()}";

        protected static void SaveDisk(string url, byte[] data)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DiskCachePath(url)));
            File.WriteAllBytes(DiskCachePath(url), data);
        }
        protected static byte[] LoadDisk(string url)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DiskCachePath(url)));
            if (!DiskCacheContains(url))
                return null;
            return File.ReadAllBytes(DiskCachePath(url));
        }
        protected static Task SaveDiskAsync(string url, byte[] data, bool suppressMessage = false)
        {
            if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Trace) && !suppressMessage)
                Debug.Log($"[ImageLoader] Save to Disk cache ({typeof(T).Name})\n{url}");
            return diskTaskFactory.StartNew(() => SaveDisk(url, data));
        }
        protected static Task<byte[]> LoadDiskAsync(string url, bool suppressMessage = false)
        {
            if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Trace) && !suppressMessage)
                Debug.Log($"[ImageLoader] Load from Disk cache ({typeof(T).Name})\n{url}");
            return diskTaskFactory.StartNew(() => LoadDisk(url));
        }

        /// <summary>
        /// Check if the image is cached at Disk
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns true if image is cached at Disk</returns>
        public static bool DiskCacheContains(string url) => File.Exists(DiskCachePath(url));

        /// <summary>
        /// Check if the image is cached at Disk
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns true if image is cached at Disk</returns>
        public static Task<bool> DiskCacheExistsAsync(string url)
        {
            var path = DiskCachePath(url);
            return diskTaskFactory.StartNew(() => File.Exists(path));
        }

        /// <summary>
        /// Clear Disk cache for all urls
        /// </summary>
        public static Task ClearDiskCache()
        {
            if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Log))
                Debug.Log($"[ImageLoader] Clear Disk cache ({typeof(T).Name}) All");
            return diskTaskFactory.StartNew(() =>
            {
                if (Directory.Exists(DiskCacheFolderPath))
                    Directory.Delete(DiskCacheFolderPath, true);
            });
        }

        /// <summary>
        /// Clear Disk cache for the given url
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        public static Task ClearDiskCache(string url)
        {
            if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Log))
                Debug.Log($"[ImageLoader] Clear Disk cache ({typeof(T).Name})\n{url}");
            var diskPath = DiskCachePath(url);
            return diskTaskFactory.StartNew(() =>
            {
                if (!File.Exists(diskPath))
                    return;
                File.Delete(diskPath);
            });
        }
    }
}
