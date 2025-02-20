using NUnit.Framework;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using System.Collections;
using Extensions.Unity.ImageLoader.Tests.Utils;

namespace Extensions.Unity.ImageLoader.Tests
{
    public partial class TestLoading : Test
    {
        public async UniTask LoadSprite(string url)
        {
            var sprite = await ImageLoader.LoadSprite(url);
            Assert.IsNotNull(sprite);
        }

        [UnityTest] public IEnumerator LoadSpritesCacheMemoryDisk_NoLogs() => TestUtils.RunNoLogs(LoadSpritesCacheMemoryDisk);
        [UnityTest] public IEnumerator LoadSpritesCacheMemoryDisk()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var imageURL in TestUtils.ImageURLs)
                yield return LoadSprite(imageURL).ToCoroutine();
        }
        [UnityTest] public IEnumerator LoadSpritesCacheMemory_NoLogs() => TestUtils.RunNoLogs(LoadSpritesCacheMemory);
        [UnityTest] public IEnumerator LoadSpritesCacheMemory()
        {
            ImageLoader.settings.useDiskCache = false;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var imageURL in TestUtils.ImageURLs)
                yield return LoadSprite(imageURL).ToCoroutine();
        }
        [UnityTest] public IEnumerator LoadSpritesCacheDisk_NoLogs() => TestUtils.RunNoLogs(LoadSpritesCacheDisk);
        [UnityTest] public IEnumerator LoadSpritesCacheDisk()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = false;

            foreach (var imageURL in TestUtils.ImageURLs)
                yield return LoadSprite(imageURL).ToCoroutine();
        }
        [UnityTest] public IEnumerator LoadSpritesNoCache_NoLogs() => TestUtils.RunNoLogs(LoadSpritesNoCache);
        [UnityTest] public IEnumerator LoadSpritesNoCache()
        {
            ImageLoader.settings.useDiskCache = false;
            ImageLoader.settings.useMemoryCache = false;

            foreach (var imageURL in TestUtils.ImageURLs)
                yield return LoadSprite(imageURL).ToCoroutine();
        }
    }
}