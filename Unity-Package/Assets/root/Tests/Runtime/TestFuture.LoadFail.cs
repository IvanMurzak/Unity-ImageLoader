using UnityEngine.TestTools;
using System.Collections;
using Extensions.Unity.ImageLoader.Tests.Utils;

namespace Extensions.Unity.ImageLoader.Tests
{
    public partial class TestFuture
    {
        [UnityTest] public IEnumerator LoadFailFrom_Source_NoLogs() => TestUtils.RunNoLogs(LoadFailFrom_Source);
        [UnityTest] public IEnumerator LoadFailFrom_Source()
        {
            yield return LoadFailFrom_Source(useDiskCache: true,  useMemoryCache: true,  usePlaceholder: false);
            yield return LoadFailFrom_Source(useDiskCache: true,  useMemoryCache: false, usePlaceholder: false);
            yield return LoadFailFrom_Source(useDiskCache: false, useMemoryCache: true,  usePlaceholder: false);
            yield return LoadFailFrom_Source(useDiskCache: false, useMemoryCache: false, usePlaceholder: false);

            yield return LoadFailFrom_Source(useDiskCache: true,  useMemoryCache: true,  usePlaceholder: true);
            yield return LoadFailFrom_Source(useDiskCache: true,  useMemoryCache: false, usePlaceholder: true);
            yield return LoadFailFrom_Source(useDiskCache: false, useMemoryCache: true,  usePlaceholder: true);
            yield return LoadFailFrom_Source(useDiskCache: false, useMemoryCache: false, usePlaceholder: true);
        }
        IEnumerator LoadFailFrom_Source(bool useDiskCache, bool useMemoryCache, bool usePlaceholder)
        {
            ImageLoader.settings.useDiskCache = useDiskCache;
            ImageLoader.settings.useMemoryCache = useMemoryCache;

            foreach (var url in TestUtils.IncorrectImageURLs())
                yield return TestUtils.LoadFail(url, FutureLoadingFrom.Source, usePlaceholder: usePlaceholder);

            yield return TestUtils.ClearEverything(message: null);
        }
    }
}