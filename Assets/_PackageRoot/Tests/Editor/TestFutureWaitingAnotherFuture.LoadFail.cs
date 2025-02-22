using UnityEngine.TestTools;
using System.Collections;
using Extensions.Unity.ImageLoader.Tests.Utils;

namespace Extensions.Unity.ImageLoader.Tests
{
    public partial class TestFutureWaitingAnotherFuture
    {
        [UnityTest] public IEnumerator LoadFailFrom_Source_NoLogs() => TestUtils.RunNoLogs(LoadFailFrom_Source);
        [UnityTest] public IEnumerator LoadFailFrom_Source()
        {
            yield return LoadFailFrom_Source(useDiskCache: true,  useMemoryCache: true);
            yield return LoadFailFrom_Source(useDiskCache: true,  useMemoryCache: false);
            yield return LoadFailFrom_Source(useDiskCache: false, useMemoryCache: true);
            yield return LoadFailFrom_Source(useDiskCache: false, useMemoryCache: false);
        }
        IEnumerator LoadFailFrom_Source(bool useDiskCache, bool useMemoryCache)
        {
            ImageLoader.settings.useDiskCache = useDiskCache;
            ImageLoader.settings.useMemoryCache = useMemoryCache;

            foreach (var url in TestUtils.IncorrectImageURLs())
            {
                var future = ImageLoader.LoadSprite(url);
                yield return TestUtils.LoadFail(url, FutureLoadingFrom.Source);
                future.Dispose();
            }

            yield return TestUtils.ClearEverything(message: null);
        }
    }
}