using UnityEngine.TestTools;
using System.Collections;
using Extensions.Unity.ImageLoader.Tests.Utils;

namespace Extensions.Unity.ImageLoader.Tests
{
    public partial class TestFuture
    {
        [UnityTest] public IEnumerator LoadFrom_Source_AndCancel_NoLogs() => TestUtils.RunNoLogs(LoadFrom_Source_AndCancel);
        [UnityTest] public IEnumerator LoadFrom_Source_AndCancel()
        {
            yield return LoadFrom_Source_AndCancel(useDiskCache: true,  useMemoryCache: true);
            yield return LoadFrom_Source_AndCancel(useDiskCache: true,  useMemoryCache: false);
            yield return LoadFrom_Source_AndCancel(useDiskCache: false, useMemoryCache: true);
            yield return LoadFrom_Source_AndCancel(useDiskCache: false, useMemoryCache: false);
        }
        IEnumerator LoadFrom_Source_AndCancel(bool useDiskCache, bool useMemoryCache)
        {
            ImageLoader.settings.useDiskCache = useDiskCache;
            ImageLoader.settings.useMemoryCache = useMemoryCache;

            foreach (var url in TestUtils.ImageURLs)
                yield return TestUtils.LoadAndCancel(url, FutureLoadingFrom.Source);
        }
        [UnityTest] public IEnumerator LoadFrom_MemoryCache_AndCancel_NoLogs() => TestUtils.RunNoLogs(LoadFrom_MemoryCache_AndCancel);
        [UnityTest] public IEnumerator LoadFrom_MemoryCache_AndCancel()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
            {
                yield return ImageLoader.LoadSprite(url).AsCoroutine();
                yield return TestUtils.LoadFromMemoryCacheAndCancel(url);
            }
        }

        [UnityTest] public IEnumerator LoadFrom_DiskCache_AndCancel_NoLogs() => TestUtils.RunNoLogs(LoadFrom_DiskCache_AndCancel);
        [UnityTest] public IEnumerator LoadFrom_DiskCache_AndCancel()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = false;

            foreach (var url in TestUtils.ImageURLs)
            {
                yield return ImageLoader.LoadSprite(url).AsCoroutine();
                yield return TestUtils.LoadAndCancel(url, FutureLoadingFrom.DiskCache);
            }
        }
    }
}