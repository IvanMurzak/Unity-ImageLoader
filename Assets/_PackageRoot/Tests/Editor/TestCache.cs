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

        [SetUp] public void SetUp() => ImageLoader.settings.debugLevel = DebugLevel.Log;

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
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
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
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
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
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
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
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
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
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
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
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
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
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
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
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
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
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
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
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
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