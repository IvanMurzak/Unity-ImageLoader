using System.Linq;
using UnityEngine.TestTools;
using System.Collections;
using Extensions.Unity.ImageLoader.Tests.Utils;
using UnityEngine;

namespace Extensions.Unity.ImageLoader.Tests
{
    public partial class TestFutureWaitingAnotherFuture
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
            {
                // Hold the source load in-flight so the outer future stays in the
                // LoadingFromSource state for the whole LoadAndCancel sequence. Without
                // this, the fast in-process load can complete and populate the memory
                // cache before the inner future is cancelled, so the inner future would
                // observe LoadedFromMemoryCache instead of the LoadingFromSource cancel
                // path under test — the source of the historical flake.
                TestUtils.BeginHold(url);
                var future = ImageLoader.LoadSprite(url);
                yield return TestUtils.LoadAndCancel(url, FutureLoadingFrom.Source);
                TestUtils.ReleaseHeld(url);
                future.Dispose();
            }

            yield return TestUtils.ClearEverything(message: null);
        }
        [UnityTest] public IEnumerator LoadFrom_MemoryCache_AndCancel_NoLogs() => TestUtils.RunNoLogs(LoadFrom_MemoryCache_AndCancel);
        [UnityTest] public IEnumerator LoadFrom_MemoryCache_AndCancel()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
            {
                yield return ImageLoader.LoadSprite(url).AsCoroutine();
                ImageLoader.LoadSprite(url);
                yield return TestUtils.LoadFromMemoryCacheAndCancel(url);
            }
        }

        [UnityTest] public IEnumerator LoadFrom_DiskCache_AndCancel_NoLogs() => TestUtils.RunNoLogs(LoadFrom_DiskCache_AndCancel);
        [UnityTest] public IEnumerator LoadFrom_DiskCache_AndCancel()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = false;

            // foreach (var url in TestUtils.ImageURLs.SelectMany(x => Enumerable.Repeat(x, 100)).OrderBy(x => Random.value))
            foreach (var url in TestUtils.ImageURLs)
            {
                yield return ImageLoader.LoadSprite(url).AsCoroutine();
                ImageLoader.LoadSprite(url);
                yield return TestUtils.LoadAndCancel(url, FutureLoadingFrom.DiskCache);
            }
        }
    }
}