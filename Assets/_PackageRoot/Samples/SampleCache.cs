using Extensions.Unity.ImageLoader;
using UnityEngine;

static class SampleCache
{
    static string url = "";
    static Sprite sprite;

    static void ChangeDiskCacheFolder()
    {
        ImageLoader.settings.diskSaveLocation = Application.persistentDataPath + "/myCustomFolder";
    }
    static void OverrideCache()
    {
        // Override Memory cache for specific image
        ImageLoader.SaveToMemoryCache(url, sprite);

        // Take from Memory cache for specific image if exists
        ImageLoader.LoadSpriteFromMemoryCache(url);
    }
    static void DoesCacheContainImage()
    {
        // Check if any cache contains specific image
        ImageLoader.CacheContains(url);

        // Check if Memory cache contains specific image
        ImageLoader.MemoryCacheContains(url);

        // Check if Disk cache contains specific image
        ImageLoader.DiskCacheContains(url);
    }
    static void ClearImage()
    {
        // Clear memory Memory and Disk cache
        ImageLoader.ClearCacheAll();

        // Clear only Memory cache for all images
        ImageLoader.ClearMemoryCacheAll();

        // Clear only Memory cache for specific image
        ImageLoader.ClearMemoryCache(url);

        // Clear only Disk cache for all images
        ImageLoader.ClearDiskCacheAll();

        // Clear only Disk cache for specific image
        ImageLoader.ClearDiskCache(url);
    }
}