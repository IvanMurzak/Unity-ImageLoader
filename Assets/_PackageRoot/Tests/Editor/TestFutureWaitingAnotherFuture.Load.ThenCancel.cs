using UnityEngine.TestTools;
using System.Collections;
using Extensions.Unity.ImageLoader.Tests.Utils;

namespace Extensions.Unity.ImageLoader.Tests
{
    public partial class TestFutureWaitingAnotherFuture
    {
        [UnityTest] public IEnumerator LoadFrom_Source_ThenCancel_NoLogs() => TestUtils.RunNoLogs(LoadFrom_Source_ThenCancel);
        [UnityTest] public IEnumerator LoadFrom_Source_ThenCancel()
        {
            yield return LoadFrom_Source_ThenCancel(useDiskCache: true,  useMemoryCache: true,  useGC: true);
            yield return LoadFrom_Source_ThenCancel(useDiskCache: true,  useMemoryCache: false, useGC: true);
            yield return LoadFrom_Source_ThenCancel(useDiskCache: false, useMemoryCache: true,  useGC: true);
            yield return LoadFrom_Source_ThenCancel(useDiskCache: false, useMemoryCache: false, useGC: true);

            yield return LoadFrom_Source_ThenCancel(useDiskCache: true,  useMemoryCache: true,  useGC: false);
            yield return LoadFrom_Source_ThenCancel(useDiskCache: true,  useMemoryCache: false, useGC: false);
            yield return LoadFrom_Source_ThenCancel(useDiskCache: false, useMemoryCache: true,  useGC: false);
            yield return LoadFrom_Source_ThenCancel(useDiskCache: false, useMemoryCache: false, useGC: false);
        }
        IEnumerator LoadFrom_Source_ThenCancel(bool useDiskCache, bool useMemoryCache, bool useGC)
        {
            ImageLoader.settings.useDiskCache = useDiskCache;
            ImageLoader.settings.useMemoryCache = useMemoryCache;

            foreach (var url in TestUtils.ImageURLs)
            {
                ImageLoader.LoadSprite(url);
                yield return TestUtils.LoadThenCancel(url, FutureLoadingFrom.Source, FutureLoadedFrom.Source, useGC);
            }

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
                ImageLoader.LoadSprite(url);
                yield return TestUtils.LoadFromMemoryCacheThenCancel(url, useGC: true);
            }
            // TODO: remove code duplicate
            yield return TestUtils.ClearEverything(message: null);
            foreach (var url in TestUtils.ImageURLs)
            {
                yield return ImageLoader.LoadSprite(url).AsCoroutine();
                ImageLoader.LoadSprite(url);
                yield return TestUtils.LoadFromMemoryCacheThenCancel(url, useGC: false);
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
                ImageLoader.LoadSprite(url);
                yield return TestUtils.LoadThenCancel(url, FutureLoadingFrom.DiskCache, FutureLoadedFrom.DiskCache, useGC: true);
            }
            // TODO: remove code duplicate
            yield return TestUtils.ClearEverything(message: null);
            foreach (var url in TestUtils.ImageURLs)
            {
                yield return ImageLoader.LoadSprite(url).AsCoroutine();
                ImageLoader.LoadSprite(url);
                yield return TestUtils.LoadThenCancel(url, FutureLoadingFrom.DiskCache, FutureLoadedFrom.DiskCache, useGC: false);
            }
        }
    }
}