using System;
using System.Linq;
using System.Collections;
using NUnit.Framework;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using UnityEngine;
using System.Threading.Tasks;

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

        [SetUp] public void SetUp() => ImageLoader.settings.debugLevel = DebugLevel.Log;

        [UnityTest] public IEnumerator CleanMemoryCacheNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return CleanMemoryCache();
        }
        [UnityTest] public IEnumerator CleanMemoryCache()
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

        //[UnityTest] public IEnumerator DisposeOnOutOfScope()
        //{
        //    yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
        //    ImageLoader.settings.useDiskCache = true;
        //    ImageLoader.settings.useMemoryCache = true;

        //    foreach (var url in ImageURLs)
        //    {
        //        var task = ImageLoader.LoadSpriteRef(url).AsTask();
        //        while (!task.IsCompleted)
        //            yield return null;

        //        Assert.AreEqual(1, Reference<Sprite>.Counter(url));
        //    }

        //    yield return null;
        //    System.GC.Collect(100, System.GCCollectionMode.Forced, blocking: true);
        //    yield return null;

        //    foreach (var url in ImageURLs)
        //    {
        //        Assert.AreEqual(0, Reference<Sprite>.Counter(url));
        //    }
        //}
        [UnityTest] public IEnumerator DisposeOnOutDisposingBlockNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return DisposeOnOutDisposingBlock();
        }
        [UnityTest] public IEnumerator DisposeOnOutDisposingBlock()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in ImageURLs)
            {
                var task = ImageLoader.LoadSpriteRef(url).AsTask();
                while (!task.IsCompleted)
                    yield return null;

                using (var reference = task.Result)
                {
                    Assert.AreEqual(1, Reference<Sprite>.Counter(url));
                }
                Assert.AreEqual(0, Reference<Sprite>.Counter(url));
            }
            foreach (var url in ImageURLs)
            {
                Assert.AreEqual(0, Reference<Sprite>.Counter(url));
            }
        }

        [UnityTest] public IEnumerator CleanMemoryCacheAllNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return CleanMemoryCacheAll();
        }
        [UnityTest] public IEnumerator CleanMemoryCacheAll()
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

        [UnityTest] public IEnumerator LoadOneMake1000ReferencesLaterDisposeNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadOneMake1000ReferencesLaterDispose();
        }
        [UnityTest] public IEnumerator LoadOneMake1000ReferencesLaterDispose()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = ImageURLs[0];

            var task1 = ImageLoader.LoadSpriteRef(url1).AsTask();
            while (!task1.IsCompleted)
                yield return null;

            var ref1_1 = task1.Result;
            Assert.NotNull(ref1_1);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));

            var count = 1000;
            var references = new Reference<Sprite>[count];
            for (var i = 0; i < count; i++)
            {
                var reference = ImageLoader.LoadFromMemoryCacheRef(url1);
                Assert.NotNull(reference);
                Assert.AreEqual(i + 2, Reference<Sprite>.Counter(url1));
                references[i] = reference;
            }

            ref1_1.Dispose();
            Assert.AreEqual(count, Reference<Sprite>.Counter(url1));

            for (var i = 0; i < count; i++)
            {
                references[i].Dispose();
                Assert.AreEqual(count - i - 1, Reference<Sprite>.Counter(url1));
            }
        }


        [UnityTest] public IEnumerator KeepReferenceButDisposeFutureNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return KeepReferenceButDisposeFuture();
        }
        [UnityTest] public IEnumerator KeepReferenceButDisposeFuture()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = ImageURLs[0];

            var future = ImageLoader.LoadSpriteRef(url1);
            var task1 = future.AsTask();
            while (!task1.IsCompleted)
                yield return null;

            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));

            var ref1 = task1.Result;
            Assert.NotNull(ref1);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));

            future.Dispose();
            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));

            ref1.Dispose();
            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));
        }

        [UnityTest] public IEnumerator LoadOneMake1000ReferencesImmediateDisposeNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadOneMake1000ReferencesImmediateDispose();
        }
        [UnityTest] public IEnumerator LoadOneMake1000ReferencesImmediateDispose()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = ImageURLs[0];

            var future = ImageLoader.LoadSpriteRef(url1);
            var task1 = future.AsTask();
            while (!task1.IsCompleted)
                yield return null;

            var ref1_1 = task1.Result;
            Assert.NotNull(ref1_1);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));

            var count = 1000;
            for (var i = 0; i < count; i++)
            {
                Assert.AreEqual(1, Reference<Sprite>.Counter(url1), $"ref[{i}] going to create.");

                var reference = ImageLoader.LoadFromMemoryCacheRef(url1);
                Assert.NotNull(reference);
                Assert.AreEqual(2, Reference<Sprite>.Counter(url1), $"ref[{i}] is created, but reference counter is wrong.");

                reference.Dispose();
                Assert.AreEqual(1, Reference<Sprite>.Counter(url1), $"ref[{i}] is disposed, but it should be still in memory cache because of another ref.");
            }
            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));

            future.Dispose();
            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));

            ref1_1.Dispose();
            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));
        }

        [UnityTest] public IEnumerator LoadOneMake1000ReferencesInParallelLateDisposeNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadOneMake1000ReferencesInParallelLateDispose();
        }
        [UnityTest] public IEnumerator LoadOneMake1000ReferencesInParallelLateDispose()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = ImageURLs[0];

            var task1 = ImageLoader.LoadSpriteRef(url1).AsTask();
            while (!task1.IsCompleted)
                yield return null;

            var ref1_1 = task1.Result;
            Assert.NotNull(ref1_1);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));

            var count = 1000;
            var references = new Reference<Sprite>[count];
            var tasks = Enumerable.Range(0, count)
                .Select(i => Task.Run(() =>
                {
                    var reference = ImageLoader.LoadFromMemoryCacheRef(url1);
                    Assert.NotNull(reference);
                    references[i] = reference;
                    return reference;
                }))
                .ToArray();

            yield return Task.WhenAll(tasks).AsUniTask().ToCoroutine();
            Assert.AreEqual(count + 1, Reference<Sprite>.Counter(url1));

            foreach (var reference in references)
                reference.Dispose();
            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));

            ref1_1.Dispose();
            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));
        }

        [UnityTest] public IEnumerator Load1Sprite2TimesAnd2TimesFromCacheNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return Load1Sprite2TimesAnd2TimesFromCache();
        }
        [UnityTest] public IEnumerator Load1Sprite2TimesAnd2TimesFromCache()
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

        [UnityTest] public IEnumerator Load2SpritesTimesAnd2TimesFromCacheNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return Load2SpritesTimesAnd2TimesFromCache();
        }
        [UnityTest] public IEnumerator Load2SpritesTimesAnd2TimesFromCache()
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
            Assert.NotNull(ref1_1);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));

            var task2 = ImageLoader.LoadSpriteRef(url2).AsTask();
            while (!task2.IsCompleted)
                yield return null;

            var ref2_1 = task2.Result;
            Assert.NotNull(ref2_1);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url2));

            var ref1_2 = ImageLoader.LoadFromMemoryCacheRef(url1);
            Assert.NotNull(ref1_2);
            Assert.AreEqual(2, Reference<Sprite>.Counter(url1));

            var ref2_2 = ImageLoader.LoadFromMemoryCacheRef(url2);
            Assert.NotNull(ref2_2);
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