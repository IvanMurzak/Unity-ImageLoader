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

        [UnityTest] public IEnumerator LoadSpritesCacheMemoryDiskNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadSpritesCacheMemoryDisk();
        }
        [UnityTest] public IEnumerator LoadSpritesCacheMemoryDisk()
        {
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
            ImageLoader.settings.useDiskCache = false;
            ImageLoader.settings.useMemoryCache = false;

            foreach (var imageURL in ImageURLs)
                yield return LoadSprite(imageURL).ToCoroutine();
        }
    }
}