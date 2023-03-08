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

        public async UniTask LoadSprite(string url)
        {
            var sprite = await ImageLoader.LoadSprite(url);
            Assert.IsNotNull(sprite);
        }

        [UnityTest] public IEnumerator DiskCacheEnable()
        {
            ImageLoader.ClearCache();
            ImageLoader.settings.useDiskCache = true;

            foreach (var imageURL in ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.DiskCacheExists(imageURL));
            }
        }
        [UnityTest] public IEnumerator DiskCacheDisable()
        {
            ImageLoader.ClearCache();
            ImageLoader.settings.useDiskCache = false;

            foreach (var imageURL in ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsFalse(ImageLoader.DiskCacheExists(imageURL));
            }
        }
        [UnityTest] public IEnumerator MemoryCacheEnabled()
        {
            ImageLoader.ClearCache();
            ImageLoader.settings.useMemoryCache = true;

            foreach (var imageURL in ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.MemoryCacheExists(imageURL));
            }
        }
        [UnityTest] public IEnumerator MemoryCacheDisabled()
        {
            ImageLoader.ClearCache();
            ImageLoader.settings.useMemoryCache = false;

            foreach (var imageURL in ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsFalse(ImageLoader.MemoryCacheExists(imageURL));
            }
        }
        [UnityTest] public IEnumerator ClearDiskCache()
        {
            ImageLoader.ClearCache();
            ImageLoader.settings.useDiskCache = true;

            foreach (var imageURL in ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.DiskCacheExists(imageURL));
                ImageLoader.ClearDiskCache(imageURL);
                Assert.IsFalse(ImageLoader.DiskCacheExists(imageURL));
            }
        }
        [UnityTest] public IEnumerator ClearMemoryCache()
        {
            ImageLoader.ClearCache();
            ImageLoader.settings.useMemoryCache = true;

            foreach (var imageURL in ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.MemoryCacheExists(imageURL));
                ImageLoader.ClearMemoryCache(imageURL);
                Assert.IsFalse(ImageLoader.MemoryCacheExists(imageURL));
            }
        }
        [UnityTest] public IEnumerator ClearDiskCacheAll()
        {
            ImageLoader.ClearCache();
            ImageLoader.settings.useDiskCache = true;

            foreach (var imageURL in ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.DiskCacheExists(imageURL));
            }
            ImageLoader.ClearDiskCache();
            foreach (var imageURL in ImageURLs)
                Assert.IsFalse(ImageLoader.DiskCacheExists(imageURL));
        }
        [UnityTest] public IEnumerator ClearMemoryCacheAll()
        {
            ImageLoader.ClearCache();
            ImageLoader.settings.useMemoryCache = true;

            foreach (var imageURL in ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.MemoryCacheExists(imageURL));
            }
            ImageLoader.ClearMemoryCache();
            foreach (var imageURL in ImageURLs)
                Assert.IsFalse(ImageLoader.MemoryCacheExists(imageURL));
        }
    }
}