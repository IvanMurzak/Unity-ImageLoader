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

        public async UniTask LoadSprite(string url)
        {
            var sprite = await ImageLoader.LoadSprite(url);
            Assert.IsNotNull(sprite);
        }

        [UnityTest] public IEnumerator LoadSpritesCacheMemoryDisk()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var imageURL in ImageURLs) 
                yield return LoadSprite(imageURL).ToCoroutine();
        }
        [UnityTest] public IEnumerator LoadSpritesCacheMemory()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = false;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var imageURL in ImageURLs) 
                yield return LoadSprite(imageURL).ToCoroutine();
        }
        [UnityTest] public IEnumerator LoadSpritesCacheDisk()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = false;

            foreach (var imageURL in ImageURLs) 
                yield return LoadSprite(imageURL).ToCoroutine();
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