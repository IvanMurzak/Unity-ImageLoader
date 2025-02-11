using NUnit.Framework;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using System.Collections;

namespace Extensions.Unity.ImageLoader.Tests
{
    public class TestLoading
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

        [UnityTest] public IEnumerator LoadSpritesCacheMemoryDiskNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadSpritesCacheMemoryDisk();
        }
        [UnityTest] public IEnumerator LoadSpritesCacheMemoryDisk()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var imageURL in ImageURLs)
                yield return LoadSprite(imageURL).ToCoroutine();
        }
        [UnityTest] public IEnumerator LoadSpritesCacheMemoryNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadSpritesCacheMemory();
        }
        [UnityTest] public IEnumerator LoadSpritesCacheMemory()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = false;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var imageURL in ImageURLs)
                yield return LoadSprite(imageURL).ToCoroutine();
        }
        [UnityTest] public IEnumerator LoadSpritesCacheDiskNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadSpritesCacheDisk();
        }
        [UnityTest] public IEnumerator LoadSpritesCacheDisk()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = false;

            foreach (var imageURL in ImageURLs)
                yield return LoadSprite(imageURL).ToCoroutine();
        }
        [UnityTest] public IEnumerator LoadSpritesNoCacheNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadSpritesNoCache();
        }
        [UnityTest] public IEnumerator LoadSpritesNoCache()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = false;
            ImageLoader.settings.useMemoryCache = false;

            foreach (var imageURL in ImageURLs)
                yield return LoadSprite(imageURL).ToCoroutine();
        }
    }
}