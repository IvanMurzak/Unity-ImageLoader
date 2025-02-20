using NUnit.Framework;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using System.Collections;
using Extensions.Unity.ImageLoader.Tests.Utils;

namespace Extensions.Unity.ImageLoader.Tests
{
    public partial class TestLoading : Test
    {
        public async UniTask LoadTextureTexture(string url)
        {
            var texture = await ImageLoader.LoadTexture(url);
            Assert.IsNotNull(texture);
        }

        [UnityTest] public IEnumerator LoadTextureTexturesCacheMemoryDisk_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadTextureTexturesCacheMemoryDisk();
        }
        [UnityTest] public IEnumerator LoadTextureTexturesCacheMemoryDisk()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var imageURL in TestUtils.ImageURLs)
                yield return LoadTextureTexture(imageURL).ToCoroutine();
        }
        [UnityTest] public IEnumerator LoadTextureTexturesCacheMemory_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadTextureTexturesCacheMemory();
        }
        [UnityTest] public IEnumerator LoadTextureTexturesCacheMemory()
        {
            ImageLoader.settings.useDiskCache = false;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var imageURL in TestUtils.ImageURLs)
                yield return LoadTextureTexture(imageURL).ToCoroutine();
        }
        [UnityTest] public IEnumerator LoadTextureTexturesCacheDisk_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadTextureTexturesCacheDisk();
        }
        [UnityTest] public IEnumerator LoadTextureTexturesCacheDisk()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = false;

            foreach (var imageURL in TestUtils.ImageURLs)
                yield return LoadTextureTexture(imageURL).ToCoroutine();
        }
        [UnityTest] public IEnumerator LoadTextureTexturesNoCache_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadTextureTexturesNoCache();
        }
        [UnityTest] public IEnumerator LoadTextureTexturesNoCache()
        {
            ImageLoader.settings.useDiskCache = false;
            ImageLoader.settings.useMemoryCache = false;

            foreach (var imageURL in TestUtils.ImageURLs)
                yield return LoadTextureTexture(imageURL).ToCoroutine();
        }
    }
}