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
        public IEnumerator LoadSpritesCacheMemoryDisk()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var task1 = ImageLoader.LoadSprite(ImageURLs[0]).AsTask();
            while (!task1.IsCompleted)
                yield return null;

            var ref0 = task1.Result;
            Assert.AreEqual(1, Reference<Sprite>.Counter(ImageURLs[0]));

            var task2 = ImageLoader.LoadSprite(ImageURLs[0]).AsTask();
            while (!task2.IsCompleted)
                yield return null;

            var ref1 = task2.Result;
            Assert.AreEqual(2, Reference<Sprite>.Counter(ImageURLs[0]));

            var ref2 = ImageLoader.LoadFromMemoryCache(ImageURLs[0]);
            Assert.AreEqual(3, Reference<Sprite>.Counter(ImageURLs[0]));

            var ref3 = ImageLoader.LoadFromMemoryCache(ImageURLs[0]);
            Assert.AreEqual(4, Reference<Sprite>.Counter(ImageURLs[0]));

            ref0.Dispose();
            Assert.AreEqual(3, Reference<Sprite>.Counter(ImageURLs[0]));
            Assert.IsNull(ref0.Value);
            Assert.IsNotNull(ref1.Value);
            Assert.IsNotNull(ref2.Value);
            Assert.IsNotNull(ref3.Value);

            ref1.Dispose();
            Assert.AreEqual(2, Reference<Sprite>.Counter(ImageURLs[0]));
            Assert.IsNull(ref0.Value);
            Assert.IsNull(ref1.Value);
            Assert.IsNotNull(ref2.Value);
            Assert.IsNotNull(ref3.Value);

            ref2.Dispose();
            Assert.AreEqual(1, Reference<Sprite>.Counter(ImageURLs[0]));
            Assert.IsNull(ref0.Value);
            Assert.IsNull(ref1.Value);
            Assert.IsNull(ref2.Value);
            Assert.IsNotNull(ref3.Value);

            ref3.Dispose();
            Assert.AreEqual(0, Reference<Sprite>.Counter(ImageURLs[0]));
            Assert.IsNull(ref0.Value);
            Assert.IsNull(ref1.Value);
            Assert.IsNull(ref2.Value);
            Assert.IsNull(ref3.Value);
        }
    }
}