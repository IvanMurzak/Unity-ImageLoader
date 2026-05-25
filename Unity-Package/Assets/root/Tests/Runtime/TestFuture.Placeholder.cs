using UnityEngine.TestTools;
using System.Collections;
using Extensions.Unity.ImageLoader.Tests.Utils;

namespace Extensions.Unity.ImageLoader.Tests
{
    public partial class TestFuture
    {
        // [UnityTest] public IEnumerator Placeholder_Source_NoLogs() => TestUtils.RunNoLogs(Placeholder_Source);
        // [UnityTest] public IEnumerator Placeholder_Source()
        // {
        //     yield return Placeholder_Source(useDiskCache: true,  useMemoryCache: true);
        //     yield return Placeholder_Source(useDiskCache: true,  useMemoryCache: false);
        //     yield return Placeholder_Source(useDiskCache: false, useMemoryCache: true);
        //     yield return Placeholder_Source(useDiskCache: false, useMemoryCache: false);
        // }
        // IEnumerator Placeholder_Source(bool useDiskCache, bool useMemoryCache)
        // {
        //     ImageLoader.settings.useDiskCache = useDiskCache;
        //     ImageLoader.settings.useMemoryCache = useMemoryCache;

        //     foreach (var url in TestUtils.ImageURLs)
        //         yield return TestUtils.Placeholder(url, FutureLoadingFrom.Source, FutureLoadedFrom.Source);

        //     yield return TestUtils.ClearEverything(message: null);
        // }

        // [UnityTest] public IEnumerator Placeholder_DiskCache_NoLogs() => TestUtils.RunNoLogs(Placeholder_DiskCache);
        // [UnityTest] public IEnumerator Placeholder_DiskCache()
        // {
        //     ImageLoader.settings.useDiskCache = true;
        //     ImageLoader.settings.useMemoryCache = false;

        //     foreach (var url in TestUtils.ImageURLs)
        //     {
        //         yield return ImageLoader.LoadSprite(url).AsCoroutine();
        //         yield return TestUtils.Placeholder(url, FutureLoadingFrom.DiskCache, FutureLoadedFrom.DiskCache);
        //     }
        // }

        // [UnityTest] public IEnumerator Placeholder_MemoryCache_NoLogs() => TestUtils.RunNoLogs(Placeholder_MemoryCache);
        // [UnityTest] public IEnumerator Placeholder_MemoryCache()
        // {
        //     ImageLoader.settings.useDiskCache = true;
        //     ImageLoader.settings.useMemoryCache = true;

        //     foreach (var url in TestUtils.ImageURLs)
        //     {
        //         yield return ImageLoader.LoadSprite(url).AsCoroutine();
        //         yield return TestUtils.LoadFromMemoryCache(url);
        //     }
        // }
    }
}