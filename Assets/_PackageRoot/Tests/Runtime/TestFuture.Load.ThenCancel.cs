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
            yield return LoadFrom_Source_ThenCancel(useDiskCache: true,  useMemoryCache: true,  useGC: true, usePlaceholder: false);
            yield return LoadFrom_Source_ThenCancel(useDiskCache: true,  useMemoryCache: false, useGC: true, usePlaceholder: false);
            yield return LoadFrom_Source_ThenCancel(useDiskCache: false, useMemoryCache: true,  useGC: true, usePlaceholder: false);
            yield return LoadFrom_Source_ThenCancel(useDiskCache: false, useMemoryCache: false, useGC: true, usePlaceholder: false);

            yield return LoadFrom_Source_ThenCancel(useDiskCache: true,  useMemoryCache: true,  useGC: false, usePlaceholder: false);
            yield return LoadFrom_Source_ThenCancel(useDiskCache: true,  useMemoryCache: false, useGC: false, usePlaceholder: false);
            yield return LoadFrom_Source_ThenCancel(useDiskCache: false, useMemoryCache: true,  useGC: false, usePlaceholder: false);
            yield return LoadFrom_Source_ThenCancel(useDiskCache: false, useMemoryCache: false, useGC: false, usePlaceholder: false);

            yield return LoadFrom_Source_ThenCancel(useDiskCache: true,  useMemoryCache: true,  useGC: true, usePlaceholder: true);
            yield return LoadFrom_Source_ThenCancel(useDiskCache: true,  useMemoryCache: false, useGC: true, usePlaceholder: true);
            yield return LoadFrom_Source_ThenCancel(useDiskCache: false, useMemoryCache: true,  useGC: true, usePlaceholder: true);
            yield return LoadFrom_Source_ThenCancel(useDiskCache: false, useMemoryCache: false, useGC: true, usePlaceholder: true);

            yield return LoadFrom_Source_ThenCancel(useDiskCache: true,  useMemoryCache: true,  useGC: false, usePlaceholder: true);
            yield return LoadFrom_Source_ThenCancel(useDiskCache: true,  useMemoryCache: false, useGC: false, usePlaceholder: true);
            yield return LoadFrom_Source_ThenCancel(useDiskCache: false, useMemoryCache: true,  useGC: false, usePlaceholder: true);
            yield return LoadFrom_Source_ThenCancel(useDiskCache: false, useMemoryCache: false, useGC: false, usePlaceholder: true);
        }
        IEnumerator LoadFrom_Source_ThenCancel(bool useDiskCache, bool useMemoryCache, bool useGC, bool usePlaceholder)
        {
            ImageLoader.settings.useDiskCache = useDiskCache;
            ImageLoader.settings.useMemoryCache = useMemoryCache;

            foreach (var url in TestUtils.ImageURLs)
                yield return TestUtils.LoadThenCancel(url, FutureLoadingFrom.Source, FutureLoadedFrom.Source, useGC: useGC, usePlaceholder: usePlaceholder);

            yield return TestUtils.ClearEverything(message: null);
        }
        IEnumerator LoadFrom_MemoryCache_ThenCancel(bool useGC, bool usePlaceholder)
        {
            foreach (var url in TestUtils.ImageURLs)
            {
                yield return ImageLoader.LoadSprite(url).AsCoroutine();
                yield return TestUtils.LoadFromMemoryCacheThenCancel(url, useGC: useGC, usePlaceholder: usePlaceholder);
            }
            yield return TestUtils.ClearEverything(message: null);
        }
        IEnumerator LoadFrom_DiskCache_ThenCancel(bool useGC, bool usePlaceholder)
        {
            foreach (var url in TestUtils.ImageURLs)
            {
                yield return ImageLoader.LoadSprite(url).AsCoroutine();
                yield return TestUtils.LoadThenCancel(url, FutureLoadingFrom.DiskCache, FutureLoadedFrom.DiskCache, useGC: useGC, usePlaceholder: usePlaceholder);
            }
            yield return TestUtils.ClearEverything(message: null);
        }

        [UnityTest] public IEnumerator LoadFrom_MemoryCache_ThenCancel_NoLogs() => TestUtils.RunNoLogs(LoadFrom_MemoryCache_ThenCancel);
        [UnityTest] public IEnumerator LoadFrom_MemoryCache_ThenCancel()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            yield return LoadFrom_MemoryCache_ThenCancel(useGC: true,  usePlaceholder: false);
            yield return LoadFrom_MemoryCache_ThenCancel(useGC: false, usePlaceholder: false);

            yield return LoadFrom_MemoryCache_ThenCancel(useGC: true,  usePlaceholder: true);
            yield return LoadFrom_MemoryCache_ThenCancel(useGC: false, usePlaceholder: true);
        }

        [UnityTest] public IEnumerator LoadFrom_DiskCache_ThenCancel_NoLogs() => TestUtils.RunNoLogs(LoadFrom_DiskCache_ThenCancel);
        [UnityTest] public IEnumerator LoadFrom_DiskCache_ThenCancel()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = false;

            yield return LoadFrom_DiskCache_ThenCancel(useGC: true,  usePlaceholder: false);
            yield return LoadFrom_DiskCache_ThenCancel(useGC: false, usePlaceholder: false);

            yield return LoadFrom_DiskCache_ThenCancel(useGC: true,  usePlaceholder: true);
            yield return LoadFrom_DiskCache_ThenCancel(useGC: false, usePlaceholder: true);
        }
    }
}