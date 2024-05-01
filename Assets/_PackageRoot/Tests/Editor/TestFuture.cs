using NUnit.Framework;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine;

namespace Extensions.Unity.ImageLoader.Tests
{
    public class TestFuture
    {
        static readonly string[] ImageURLs =
        {
            "https://github.com/IvanMurzak/Unity-ImageLoader/raw/master/Test%20Images/ImageA.jpg",
            "https://github.com/IvanMurzak/Unity-ImageLoader/raw/master/Test%20Images/ImageB.png",
            "https://github.com/IvanMurzak/Unity-ImageLoader/raw/master/Test%20Images/ImageC.png"
        };

        [SetUp] public void SetUp() => ImageLoader.settings.debugLevel = DebugLevel.Log;

        [UnityTest] public IEnumerator LoadingRefAndWaiting()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = ImageURLs[0];

            var task1 = ImageLoader.LoadSpriteRef(url1).AsTask();
            while (!task1.IsCompleted)
                yield return null;

            var ref0 = task1.Result;
            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));

            ImageLoader.ClearMemoryCache(url1);
            Assert.IsNull(ref0.Value);
            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));

            ref0.Dispose();
            Assert.IsNull(ref0.Value);
            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));
        }

        [UnityTest] public IEnumerator Loading2RefAndCancelFirst()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = ImageURLs[0];

            var future1 = ImageLoader.LoadSpriteRef(url1);
            var future2 = ImageLoader.LoadSpriteRef(url1);

            future1.Cancel();

            var task2 = future2.AsTask();
            while (!task2.IsCompleted)
                yield return null;

            var ref2 = task2.Result;
            Assert.IsNotNull(ref2.Value);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));
        }

        [UnityTest] public IEnumerator Loading2RefAndWait()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = ImageURLs[0];

            var task1 = ImageLoader.LoadSpriteRef(url1).AsTask();
            var task2 = ImageLoader.LoadSpriteRef(url1).AsTask();

            while (!task1.IsCompleted || !task2.IsCompleted)
                yield return null;

            var ref0 = task1.Result;
            var ref1 = task2.Result;
            Assert.AreEqual(2, Reference<Sprite>.Counter(url1));

            ImageLoader.ClearMemoryCache(url1);
            Assert.IsNull(ref0.Value);
            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));

            ref0.Dispose();
            ref1.Dispose();
            Assert.IsNull(ref1.Value);
            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));
        }

        [UnityTest] public IEnumerator Loading2RefAndDisposeAll()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = ImageURLs[0];

            var future1 = ImageLoader.LoadSpriteRef(url1);
            var future2 = ImageLoader.LoadSpriteRef(url1);

            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));

            future1.Cancel();
            future2.Cancel();

            future1.Dispose();
            future2.Dispose();

            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));
        }

        [UnityTest] public IEnumerator DisposeOnOutDisposingBlock()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in ImageURLs)
            {
                var future1 = ImageLoader.LoadSpriteRef(url);
                var task1 = future1.AsTask();
                while (!task1.IsCompleted)
                    yield return null;

                Assert.AreEqual(1, Reference<Sprite>.Counter(url));
                future1.Dispose();
                Assert.AreEqual(0, Reference<Sprite>.Counter(url));

                using (var future2 = ImageLoader.LoadSpriteRef(url))
                {
                    var task2 = future2.AsTask();
                    while (!task2.IsCompleted)
                        yield return null;
                    Assert.AreEqual(1, Reference<Sprite>.Counter(url));
                }
                Assert.AreEqual(0, Reference<Sprite>.Counter(url));
            }
            foreach (var url in ImageURLs)
            {
                Assert.AreEqual(0, Reference<Sprite>.Counter(url));
            }
        }
    }
}