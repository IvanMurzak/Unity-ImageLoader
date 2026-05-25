using System.Collections;
using Extensions.Unity.ImageLoader.Tests.Utils;
using System;

namespace Extensions.Unity.ImageLoader.Tests
{
    public class Test
    {
        public virtual IEnumerator SetUp()
        {
            yield return TestUtils.ClearEverything("<b>Test Start </b>");
            ImageLoader.settings.debugLevel = DebugLevel.Trace;
        }
        public virtual IEnumerator TearDown()
        {
            yield return TestUtils.WaitWhile(() => ImageLoader.GetLoadingSpriteFutures().Count > 0, TimeSpan.FromSeconds(10));
            yield return TestUtils.WaitWhile(() => ImageLoader.GetLoadingTextureFutures().Count > 0, TimeSpan.FromSeconds(10));
            yield return TestUtils.ClearEverything("<b>Test End </b>");
        }
    }
}