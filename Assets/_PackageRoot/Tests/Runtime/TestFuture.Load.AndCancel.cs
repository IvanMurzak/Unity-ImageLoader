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
            yield return LoadFrom_Source_AndCancel(useDiskCache: true,  useMemoryCache: true,  usePlaceholder: false);
            yield return LoadFrom_Source_AndCancel(useDiskCache: true,  useMemoryCache: false, usePlaceholder: false);
            yield return LoadFrom_Source_AndCancel(useDiskCache: false, useMemoryCache: true,  usePlaceholder: false);
            yield return LoadFrom_Source_AndCancel(useDiskCache: false, useMemoryCache: false, usePlaceholder: false);

            yield return LoadFrom_Source_AndCancel(useDiskCache: true,  useMemoryCache: true,  usePlaceholder: true);
            yield return LoadFrom_Source_AndCancel(useDiskCache: true,  useMemoryCache: false, usePlaceholder: true);
            yield return LoadFrom_Source_AndCancel(useDiskCache: false, useMemoryCache: true,  usePlaceholder: true);
            yield return LoadFrom_Source_AndCancel(useDiskCache: false, useMemoryCache: false, usePlaceholder: true);
        }
        IEnumerator LoadFrom_Source_AndCancel(bool useDiskCache, bool useMemoryCache, bool usePlaceholder)
        {
            ImageLoader.settings.useDiskCache = useDiskCache;
            ImageLoader.settings.useMemoryCache = useMemoryCache;

            foreach (var url in TestUtils.ImageURLs)
                yield return TestUtils.LoadAndCancel(url, FutureLoadingFrom.Source);

            yield return TestUtils.ClearEverything(message: null);
        }
        IEnumerator LoadFrom_MemoryCache_AndCancel(bool usePlaceholder)
        {
            foreach (var url in TestUtils.ImageURLs)
            {
                yield return ImageLoader.LoadSprite(url).AsCoroutine();
                yield return TestUtils.LoadFromMemoryCacheAndCancel(url, usePlaceholder: usePlaceholder);
            }
            yield return TestUtils.ClearEverything(message: null);
        }
        IEnumerator LoadFrom_DiskCache_AndCancel(bool usePlaceholder)
        {
            foreach (var url in TestUtils.ImageURLs)
            {
                yield return ImageLoader.LoadSprite(url).AsCoroutine();
                yield return TestUtils.LoadAndCancel(url, FutureLoadingFrom.DiskCache, usePlaceholder: usePlaceholder);
            }
            yield return TestUtils.ClearEverything(message: null);
        }
        [UnityTest] public IEnumerator LoadFrom_MemoryCache_AndCancel_NoLogs() => TestUtils.RunNoLogs(LoadFrom_MemoryCache_AndCancel);
        [UnityTest] public IEnumerator LoadFrom_MemoryCache_AndCancel()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            yield return LoadFrom_MemoryCache_AndCancel(usePlaceholder: false);
            yield return LoadFrom_MemoryCache_AndCancel(usePlaceholder: true);
        }

        [UnityTest] public IEnumerator LoadFrom_DiskCache_AndCancel_NoLogs() => TestUtils.RunNoLogs(LoadFrom_DiskCache_AndCancel);
        [UnityTest] public IEnumerator LoadFrom_DiskCache_AndCancel()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = false;

            yield return LoadFrom_DiskCache_AndCancel(usePlaceholder: false);
            yield return LoadFrom_DiskCache_AndCancel(usePlaceholder: true);
        }
    }
}