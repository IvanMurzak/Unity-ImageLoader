using NUnit.Framework;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine;

namespace Extensions.Unity.ImageLoader.Tests
{
    public class TestReference
    {
        static readonly string[] ImageURLs =
        {
            "https://github.com/IvanMurzak/Unity-ImageLoader/raw/master/Test%20Images/ImageA.jpg",
            "https://github.com/IvanMurzak/Unity-ImageLoader/raw/master/Test%20Images/ImageB.png",
            "https://github.com/IvanMurzak/Unity-ImageLoader/raw/master/Test%20Images/ImageC.png"
        };

        [UnityTest]
        public IEnumerator CleanMemoryCache()
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

        [UnityTest]
        public IEnumerator CleanMemoryCacheAll()
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

            ImageLoader.ClearMemoryCache();
            Assert.IsNull(ref0.Value);
            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));

            ref0.Dispose();
            Assert.IsNull(ref0.Value);
            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));
        }

        [UnityTest]
        public IEnumerator Load1Sprite2TimesAnd2TimesFromCache()
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

            var task2 = ImageLoader.LoadSpriteRef(url1).AsTask();
            while (!task2.IsCompleted)
                yield return null;

            var ref1 = task2.Result;
            Assert.AreEqual(2, Reference<Sprite>.Counter(url1));

            var ref2 = ImageLoader.LoadFromMemoryCacheRef(url1);
            Assert.AreEqual(3, Reference<Sprite>.Counter(url1));

            var ref3 = ImageLoader.LoadFromMemoryCacheRef(url1);
            Assert.AreEqual(4, Reference<Sprite>.Counter(url1));

            ref0.Dispose();
            Assert.AreEqual(3, Reference<Sprite>.Counter(url1));
            Assert.IsNull(ref0.Value);
            Assert.IsNotNull(ref1.Value);
            Assert.IsNotNull(ref2.Value);
            Assert.IsNotNull(ref3.Value);

            ref1.Dispose();
            Assert.AreEqual(2, Reference<Sprite>.Counter(url1));
            Assert.IsNull(ref0.Value);
            Assert.IsNull(ref1.Value);
            Assert.IsNotNull(ref2.Value);
            Assert.IsNotNull(ref3.Value);

            ref2.Dispose();
            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));
            Assert.IsNull(ref0.Value);
            Assert.IsNull(ref1.Value);
            Assert.IsNull(ref2.Value);
            Assert.IsNotNull(ref3.Value);

            ref3.Dispose();
            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));
            Assert.IsNull(ref0.Value);
            Assert.IsNull(ref1.Value);
            Assert.IsNull(ref2.Value);
            Assert.IsNull(ref3.Value);
        }

        [UnityTest]
        public IEnumerator Load2SpritesTimesAnd2TimesFromCache()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = ImageURLs[0];
            var url2 = ImageURLs[1];

            var task1 = ImageLoader.LoadSpriteRef(url1).AsTask();
            while (!task1.IsCompleted)
                yield return null;

            var ref1_1 = task1.Result;
            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));

            var task2 = ImageLoader.LoadSpriteRef(url2).AsTask();
            while (!task2.IsCompleted)
                yield return null;

            var ref2_1 = task2.Result;
            Assert.AreEqual(1, Reference<Sprite>.Counter(url2));

            var ref1_2 = ImageLoader.LoadFromMemoryCacheRef(url1);
            Assert.AreEqual(2, Reference<Sprite>.Counter(url1));

            var ref2_2 = ImageLoader.LoadFromMemoryCacheRef(url2);
            Assert.AreEqual(2, Reference<Sprite>.Counter(url2));

            ref1_1.Dispose();
            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));
            Assert.AreEqual(2, Reference<Sprite>.Counter(url2));
            Assert.IsNull(ref1_1.Value);
            Assert.IsNotNull(ref2_1.Value);
            Assert.IsNotNull(ref1_2.Value);
            Assert.IsNotNull(ref2_2.Value);

            ref2_1.Dispose();
            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));
            Assert.AreEqual(1, Reference<Sprite>.Counter(url2));
            Assert.IsNull(ref1_1.Value);
            Assert.IsNull(ref2_1.Value);
            Assert.IsNotNull(ref1_2.Value);
            Assert.IsNotNull(ref2_2.Value);

            ref1_2.Dispose();
            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));
            Assert.AreEqual(1, Reference<Sprite>.Counter(url2));
            Assert.IsNull(ref1_1.Value);
            Assert.IsNull(ref2_1.Value);
            Assert.IsNull(ref1_2.Value);
            Assert.IsNotNull(ref2_2.Value);

            ref2_2.Dispose();
            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));
            Assert.AreEqual(0, Reference<Sprite>.Counter(url2));
            Assert.IsNull(ref1_1.Value);
            Assert.IsNull(ref2_1.Value);
            Assert.IsNull(ref1_2.Value);
            Assert.IsNull(ref2_2.Value);
        }
    }
}