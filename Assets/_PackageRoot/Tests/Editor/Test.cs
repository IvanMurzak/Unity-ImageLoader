using System.Collections;
using NUnit.Framework;
using Extensions.Unity.ImageLoader.Tests.Utils;

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
            Assert.Zero(ImageLoader.GetLoadingSpriteFutures().Count);
            Assert.Zero(ImageLoader.GetLoadingTextureFutures().Count);
            yield return TestUtils.ClearEverything("<b>Test End </b>");
        }
    }
}