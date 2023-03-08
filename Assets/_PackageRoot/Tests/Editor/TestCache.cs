using System;
using UnityEngine;
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
            Assert.AreNotEqual(sprite, null);
        }

        [UnityTest] public IEnumerator DiskCache()
        {
            ImageLoader.ClearCache();

            foreach (var imageURL in ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.DiskCacheExists(imageURL));
            }
        }
        [UnityTest] public IEnumerator MemoryCache()
        {
            ImageLoader.ClearCache();

            foreach (var imageURL in ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.MemoryCacheExists(imageURL));
            }
        }
        [UnityTest] public IEnumerator ClearDiskCache()
        {
            ImageLoader.ClearCache();

            foreach (var imageURL in ImageURLs)
            {
                yield return LoadSprite(imageURL).ToCoroutine();
                Assert.IsTrue(ImageLoader.DiskCacheExists(imageURL));
            }
            ImageLoader.ClearDiskCache();
            foreach (var imageURL in ImageURLs)
                Assert.IsFalse(ImageLoader.DiskCacheExists(imageURL));
        }
        [UnityTest] public IEnumerator ClearMemoryCache()
        {
            ImageLoader.ClearCache();

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