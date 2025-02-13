using NUnit.Framework;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using System.Collections;

namespace Extensions.Unity.ImageLoader.Tests
{
    public class TestCache
    {
        static readonly string[] ImageURLs =
        {
            "https://github.com/IvanMurzak/Unity-ImageLoader/raw/master/Test%20Images/ImageA.jpg",
            "https://github.com/IvanMurzak/Unity-ImageLoader/raw/master/Test%20Images/ImageB.png",
            "https://github.com/IvanMurzak/Unity-ImageLoader/raw/master/Test%20Images/ImageC.png"
        };

        [UnitySetUp] public IEnumerator SetUp()
        {
            yield return TestUtils.ClearEverything("<b>Test Start </b>");
            ImageLoader.settings.debugLevel = DebugLevel.Log;
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            Assert.Zero(ImageLoader.GetLoadingFutures().Count);
            yield return TestUtils.ClearEverything("<b>Test End </b>");
        }

        public async UniTask LoadSprite(string url)
        {
            var sprite = await ImageLoader.LoadSprite(url);
            Assert.IsNotNull(sprite);
        }

        [UnityTest] public IEnumerator LoadingFromMemoryCacheNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadingFromMemoryCache();
        }
        [UnityTest] public IEnumerator LoadingFromMemoryCache()
        {
            ImageLoader.settings.useMemoryCache = true;
            ImageLoader.settings.useDiskCache = false;

            foreach (var imageURL in ImageURLs)
            {
                Assert.IsFalse(ImageLoader.MemoryCacheContains(imageURL));
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.MemoryCacheContains(imageURL));
            }
        }
        [UnityTest] public IEnumerator LoadingFromDiskCacheNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadingFromDiskCache();
        }
        [UnityTest] public IEnumerator LoadingFromDiskCache()
        {
            ImageLoader.settings.useMemoryCache = false;
            ImageLoader.settings.useDiskCache = true;

            foreach (var imageURL in ImageURLs)
            {
                Assert.IsFalse(ImageLoader.DiskCacheContains(imageURL));
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.DiskCacheContains(imageURL));
            }
        }
        [UnityTest] public IEnumerator DiskCacheEnableNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return DiskCacheEnable();
        }
        [UnityTest] public IEnumerator DiskCacheEnable()
        {
            ImageLoader.settings.useDiskCache = true;

            foreach (var imageURL in ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.DiskCacheContains(imageURL));
            }
        }
        [UnityTest] public IEnumerator DiskCacheDisableNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return DiskCacheDisable();
        }
        [UnityTest] public IEnumerator DiskCacheDisable()
        {
            ImageLoader.settings.useDiskCache = false;

            foreach (var imageURL in ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsFalse(ImageLoader.DiskCacheContains(imageURL));
            }
        }
        [UnityTest] public IEnumerator MemoryCacheEnabledNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return MemoryCacheEnabled();
        }
        [UnityTest] public IEnumerator MemoryCacheEnabled()
        {
            ImageLoader.settings.useMemoryCache = true;

            foreach (var imageURL in ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.MemoryCacheContains(imageURL));
            }
        }
        [UnityTest] public IEnumerator MemoryCacheDisabledNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return MemoryCacheDisabled();
        }
        [UnityTest] public IEnumerator MemoryCacheDisabled()
        {
            ImageLoader.settings.useMemoryCache = false;

            foreach (var imageURL in ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsFalse(ImageLoader.MemoryCacheContains(imageURL));
            }
        }
        [UnityTest] public IEnumerator ClearDiskCacheNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return ClearDiskCache();
        }
        [UnityTest] public IEnumerator ClearDiskCache()
        {
            ImageLoader.settings.useDiskCache = true;

            foreach (var imageURL in ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.DiskCacheContains(imageURL));
                yield return ImageLoader.ClearDiskCache().AsUniTask().ToCoroutine();
                Assert.IsFalse(ImageLoader.DiskCacheContains(imageURL));
            }
        }
        [UnityTest] public IEnumerator ClearMemoryCacheNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return ClearMemoryCache();
        }
        [UnityTest] public IEnumerator ClearMemoryCache()
        {
            ImageLoader.settings.useMemoryCache = true;

            foreach (var imageURL in ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.MemoryCacheContains(imageURL));
                ImageLoader.ClearMemoryCache(imageURL);
                Assert.IsFalse(ImageLoader.MemoryCacheContains(imageURL));
            }
        }
        [UnityTest] public IEnumerator ClearDiskCacheAllNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return ClearDiskCacheAll();
        }
        [UnityTest] public IEnumerator ClearDiskCacheAll()
        {
            ImageLoader.settings.useDiskCache = true;

            foreach (var imageURL in ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.DiskCacheContains(imageURL));
            }
            yield return ImageLoader.ClearDiskCache().AsUniTask().ToCoroutine();
            foreach (var imageURL in ImageURLs)
                Assert.IsFalse(ImageLoader.DiskCacheContains(imageURL));
        }
        [UnityTest] public IEnumerator ClearMemoryCacheAllNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return ClearMemoryCacheAll();
        }
        [UnityTest] public IEnumerator ClearMemoryCacheAll()
        {
            ImageLoader.settings.useMemoryCache = true;

            foreach (var imageURL in ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.MemoryCacheContains(imageURL));
            }
            ImageLoader.ClearMemoryCache();
            foreach (var imageURL in ImageURLs)
                Assert.IsFalse(ImageLoader.MemoryCacheContains(imageURL));
        }
    }
}