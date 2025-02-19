using NUnit.Framework;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using System.Collections;
using Extensions.Unity.ImageLoader.Tests.Utils;

namespace Extensions.Unity.ImageLoader.Tests
{
    public class TestCache : Test
    {
        [UnitySetUp] public override IEnumerator SetUp() => base.SetUp();
        [UnityTearDown] public override IEnumerator TearDown() => base.TearDown();

        public async UniTask LoadSprite(string url)
        {
            var sprite = await ImageLoader.LoadSprite(url);
            Assert.IsNotNull(sprite);
        }

        [UnityTest] public IEnumerator LoadingFromMemoryCache_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadingFromMemoryCache();
        }
        [UnityTest] public IEnumerator LoadingFromMemoryCache()
        {
            ImageLoader.settings.useMemoryCache = true;
            ImageLoader.settings.useDiskCache = false;

            foreach (var imageURL in TestUtils.ImageURLs)
            {
                Assert.IsFalse(ImageLoader.MemoryCacheContains(imageURL));
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.MemoryCacheContains(imageURL));
            }
        }
        [UnityTest] public IEnumerator LoadingFromDiskCache_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadingFromDiskCache();
        }
        [UnityTest] public IEnumerator LoadingFromDiskCache()
        {
            ImageLoader.settings.useMemoryCache = false;
            ImageLoader.settings.useDiskCache = true;

            foreach (var imageURL in TestUtils.ImageURLs)
            {
                Assert.IsFalse(ImageLoader.DiskCacheContains(imageURL));
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.DiskCacheContains(imageURL));
            }
        }
        [UnityTest] public IEnumerator DiskCacheEnable_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return DiskCacheEnable();
        }
        [UnityTest] public IEnumerator DiskCacheEnable()
        {
            ImageLoader.settings.useDiskCache = true;

            foreach (var imageURL in TestUtils.ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.DiskCacheContains(imageURL));
            }
        }
        [UnityTest] public IEnumerator DiskCacheDisable_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return DiskCacheDisable();
        }
        [UnityTest] public IEnumerator DiskCacheDisable()
        {
            ImageLoader.settings.useDiskCache = false;

            foreach (var imageURL in TestUtils.ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsFalse(ImageLoader.DiskCacheContains(imageURL));
            }
        }
        [UnityTest] public IEnumerator MemoryCacheEnabled_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return MemoryCacheEnabled();
        }
        [UnityTest] public IEnumerator MemoryCacheEnabled()
        {
            ImageLoader.settings.useMemoryCache = true;

            foreach (var imageURL in TestUtils.ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.MemoryCacheContains(imageURL));
            }
        }
        [UnityTest] public IEnumerator MemoryCacheDisabled_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return MemoryCacheDisabled();
        }
        [UnityTest] public IEnumerator MemoryCacheDisabled()
        {
            ImageLoader.settings.useMemoryCache = false;

            foreach (var imageURL in TestUtils.ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsFalse(ImageLoader.MemoryCacheContains(imageURL));
            }
        }
        [UnityTest] public IEnumerator ClearDiskCache_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return ClearDiskCache();
        }
        [UnityTest] public IEnumerator ClearDiskCache()
        {
            ImageLoader.settings.useDiskCache = true;

            foreach (var imageURL in TestUtils.ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.DiskCacheContains(imageURL));
                yield return ImageLoader.ClearDiskCacheAll().AsUniTask().ToCoroutine();
                Assert.IsFalse(ImageLoader.DiskCacheContains(imageURL));
            }
        }
        [UnityTest] public IEnumerator ClearMemoryCache_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return ClearMemoryCache();
        }
        [UnityTest] public IEnumerator ClearMemoryCache()
        {
            ImageLoader.settings.useMemoryCache = true;

            foreach (var imageURL in TestUtils.ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.MemoryCacheContains(imageURL));
                ImageLoader.ClearMemoryCache(imageURL);
                Assert.IsFalse(ImageLoader.MemoryCacheContains(imageURL));
            }
        }
        [UnityTest] public IEnumerator ClearDiskCacheAll_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return ClearDiskCacheAll();
        }
        [UnityTest] public IEnumerator ClearDiskCacheAll()
        {
            ImageLoader.settings.useDiskCache = true;

            foreach (var imageURL in TestUtils.ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.DiskCacheContains(imageURL));
            }
            yield return ImageLoader.ClearDiskCacheAll().AsUniTask().ToCoroutine();
            foreach (var imageURL in TestUtils.ImageURLs)
                Assert.IsFalse(ImageLoader.DiskCacheContains(imageURL));
        }
        [UnityTest] public IEnumerator ClearMemoryCacheAll_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return ClearMemoryCacheAll();
        }
        [UnityTest] public IEnumerator ClearMemoryCacheAll()
        {
            ImageLoader.settings.useMemoryCache = true;

            foreach (var imageURL in TestUtils.ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.MemoryCacheContains(imageURL));
            }
            ImageLoader.ClearMemoryCacheAll();
            foreach (var imageURL in TestUtils.ImageURLs)
                Assert.IsFalse(ImageLoader.MemoryCacheContains(imageURL));
        }
    }
}