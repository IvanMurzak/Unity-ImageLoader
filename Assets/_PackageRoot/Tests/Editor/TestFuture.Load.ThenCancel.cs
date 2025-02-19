using UnityEngine.TestTools;
using System.Collections;
using Extensions.Unity.ImageLoader.Tests.Utils;

namespace Extensions.Unity.ImageLoader.Tests
{
    public partial class TestFuture
    {
        [UnityTest] public IEnumerator LoadFrom_Source_ThenCancel_NoLogs() => TestUtils.RunNoLogs(LoadFrom_Source_ThenCancel);
        [UnityTest] public IEnumerator LoadFrom_Source_ThenCancel()
        {
            yield return LoadFrom_Source_ThenCancel(useDiskCache: true,  useMemoryCache: true);
            yield return LoadFrom_Source_ThenCancel(useDiskCache: true,  useMemoryCache: false);
            yield return LoadFrom_Source_ThenCancel(useDiskCache: false, useMemoryCache: true);
            yield return LoadFrom_Source_ThenCancel(useDiskCache: false, useMemoryCache: false);
        }
        IEnumerator LoadFrom_Source_ThenCancel(bool useDiskCache, bool useMemoryCache)
        {
            ImageLoader.settings.useDiskCache = useDiskCache;
            ImageLoader.settings.useMemoryCache = useMemoryCache;

            foreach (var url in TestUtils.ImageURLs)
                yield return TestUtils.LoadThenCancel(url, FutureLoadingFrom.Source, FutureLoadedFrom.Source);

            yield return TestUtils.ClearEverything(message: null);
        }

        [UnityTest] public IEnumerator LoadFrom_MemoryCache_ThenCancel_NoLogs() => TestUtils.RunNoLogs(LoadFrom_MemoryCache_ThenCancel);
        [UnityTest] public IEnumerator LoadFrom_MemoryCache_ThenCancel()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
            {
                yield return ImageLoader.LoadSprite(url).AsCoroutine();
                yield return TestUtils.LoadFromMemoryCacheThenCancel(url);
            }
        }

        [UnityTest] public IEnumerator LoadFrom_DiskCache_ThenCancel_NoLogs() => TestUtils.RunNoLogs(LoadFrom_DiskCache_ThenCancel);
        [UnityTest] public IEnumerator LoadFrom_DiskCache_ThenCancel()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = false;

            foreach (var url in TestUtils.ImageURLs)
            {
                yield return ImageLoader.LoadSprite(url).AsCoroutine();
                yield return TestUtils.LoadThenCancel(url, FutureLoadingFrom.DiskCache, FutureLoadedFrom.DiskCache);
            }
        }
    }
}