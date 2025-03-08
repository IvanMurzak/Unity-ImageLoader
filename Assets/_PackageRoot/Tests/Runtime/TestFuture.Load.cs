using UnityEngine.TestTools;
using System.Collections;
using Extensions.Unity.ImageLoader.Tests.Utils;

namespace Extensions.Unity.ImageLoader.Tests
{
    public partial class TestFuture
    {
        [UnityTest] public IEnumerator LoadFrom_Source_NoLogs() => TestUtils.RunNoLogs(LoadFrom_Source);
        [UnityTest] public IEnumerator LoadFrom_Source()
        {
            yield return LoadFrom_Source(useDiskCache: true,  useMemoryCache: true,  usePlaceholder: false);
            yield return LoadFrom_Source(useDiskCache: true,  useMemoryCache: false, usePlaceholder: false);
            yield return LoadFrom_Source(useDiskCache: false, useMemoryCache: true,  usePlaceholder: false);
            yield return LoadFrom_Source(useDiskCache: false, useMemoryCache: false, usePlaceholder: false);

            yield return LoadFrom_Source(useDiskCache: true,  useMemoryCache: true,  usePlaceholder: true);
            yield return LoadFrom_Source(useDiskCache: true,  useMemoryCache: false, usePlaceholder: true);
            yield return LoadFrom_Source(useDiskCache: false, useMemoryCache: true,  usePlaceholder: true);
            yield return LoadFrom_Source(useDiskCache: false, useMemoryCache: false, usePlaceholder: true);
        }
        IEnumerator LoadFrom_Source(bool useDiskCache, bool useMemoryCache, bool usePlaceholder)
        {
            ImageLoader.settings.useDiskCache = useDiskCache;
            ImageLoader.settings.useMemoryCache = useMemoryCache;

            foreach (var url in TestUtils.ImageURLs)
                yield return TestUtils.Load(url, FutureLoadingFrom.Source, FutureLoadedFrom.Source, usePlaceholder: usePlaceholder);

            yield return TestUtils.ClearEverything(message: null);
        }
        IEnumerator LoadFrom_DiskCache(bool usePlaceholder)
        {
            foreach (var url in TestUtils.ImageURLs)
            {
                yield return ImageLoader.LoadSprite(url).AsCoroutine();
                yield return TestUtils.Load(url, FutureLoadingFrom.DiskCache, FutureLoadedFrom.DiskCache, usePlaceholder: usePlaceholder);
            }
            yield return TestUtils.ClearEverything(message: null);
        }
        IEnumerator LoadFrom_MemoryCache(bool usePlaceholder)
        {
            foreach (var url in TestUtils.ImageURLs)
            {
                yield return ImageLoader.LoadSprite(url).AsCoroutine();
                yield return TestUtils.LoadFromMemoryCache(url, usePlaceholder: usePlaceholder);
            }
            yield return TestUtils.ClearEverything(message: null);
        }

        [UnityTest] public IEnumerator LoadFrom_DiskCache_NoLogs() => TestUtils.RunNoLogs(LoadFrom_DiskCache);
        [UnityTest] public IEnumerator LoadFrom_DiskCache()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = false;

            yield return LoadFrom_DiskCache(usePlaceholder: false);
            yield return LoadFrom_DiskCache(usePlaceholder: true);
        }

        [UnityTest] public IEnumerator LoadFrom_MemoryCache_NoLogs() => TestUtils.RunNoLogs(LoadFrom_MemoryCache);
        [UnityTest] public IEnumerator LoadFrom_MemoryCache()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            yield return LoadFrom_MemoryCache(usePlaceholder: false);
            yield return LoadFrom_MemoryCache(usePlaceholder: true);
        }
    }
}