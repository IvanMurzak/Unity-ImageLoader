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
        [UnitySetUp] public IEnumerator SetUp()
        {
            yield return TestUtils.ClearEverything("<b>Test Start </b>");
            ImageLoader.settings.debugLevel = DebugLevel.Trace;
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            Assert.Zero(ImageLoader.GetLoadingFutures().Count);
            yield return TestUtils.ClearEverything("<b>Test End </b>");
        }

        [UnityTest] public IEnumerator CleanMemoryCache_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return CleanMemoryCache();
        }
        [UnityTest] public IEnumerator CleanMemoryCache()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = TestUtils.ImageURLs[0];

            var task1 = ImageLoader.LoadSpriteRef(url1).AsTask();
            while (!task1.IsCompleted)
                yield return null;

            var ref0 = task1.Result;
            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));

            Assert.Throws<Exception>(() => ImageLoader.ClearMemoryCache(url1));
            Assert.IsNotNull(ref0.Value);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));

            ref0.Dispose();
            Assert.IsNull(ref0.Value);
            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));
        }

        [UnityTest] public IEnumerator DisposeOnOutOfScope_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return DisposeOnOutOfScope();
        }
        [UnityTest] public IEnumerator DisposeOnOutOfScope()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = TestUtils.ImageURLs[0];
            { // scope
                var task = ImageLoader.LoadSpriteRef(url).AsTask();
                while (!task.IsCompleted)
                    yield return null;

                var reference = task.Result;
                Assert.NotNull(reference);
                Assert.AreEqual(1, Reference<Sprite>.Counter(url));
            } // end of scope

            yield return TestUtils.WaitForGC();
            Assert.AreEqual(0, Reference<Sprite>.Counter(url));
        }

        [UnityTest] public IEnumerator DisposeOnOutOfScope2()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = TestUtils.ImageURLs[0];

            var futureRef = ImageLoader.LoadSpriteRef(url);
            while (futureRef.IsInProgress)
                yield return null;

            var reference = futureRef.Value;
            Assert.NotNull(reference);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url));

            futureRef = null;
            reference = null;

            yield return TestUtils.WaitForGC();
            Assert.AreEqual(0, Reference<Sprite>.Counter(url));
        }

        [UnityTest] public IEnumerator DisposeOnOutOfScopeAll_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return DisposeOnOutOfScopeAll();
        }
        [UnityTest] public IEnumerator DisposeOnOutOfScopeAll()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
            {
                var task = ImageLoader.LoadSpriteRef(url).AsTask();
                while (!task.IsCompleted)
                    yield return null;

                var reference = task.Result;
                Assert.NotNull(reference);
                Assert.AreEqual(1, Reference<Sprite>.Counter(url));
            }

            yield return TestUtils.WaitForGC();

            foreach (var url in TestUtils.ImageURLs)
                Assert.AreEqual(0, Reference<Sprite>.Counter(url));
        }
        [UnityTest] public IEnumerator DisposeOnOutDisposingBlock_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return DisposeOnOutDisposingBlock();
        }
        [UnityTest] public IEnumerator DisposeOnOutDisposingBlock()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
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
            foreach (var url in TestUtils.ImageURLs)
            {
                Assert.AreEqual(0, Reference<Sprite>.Counter(url));
            }
        }

        [UnityTest] public IEnumerator CleanMemoryCacheAll_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return CleanMemoryCacheAll();
        }
        [UnityTest] public IEnumerator CleanMemoryCacheAll()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = TestUtils.ImageURLs[0];

            var task1 = ImageLoader.LoadSpriteRef(url1).AsTask();
            while (!task1.IsCompleted)
                yield return null;

            var ref0 = task1.Result;
            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));

            LogAssert.Expect(LogType.Error, $"[ImageLoader] There are 1 references to the object, clear them first. URL={url1}");
            ImageLoader.ClearMemoryCacheAll();

            Assert.IsNotNull(ref0.Value);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));

            ref0.Dispose();
            Assert.IsNull(ref0.Value);
            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));
        }

        [UnityTest] public IEnumerator LoadOneMake1000ReferencesLaterDispose_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadOneMake1000ReferencesLaterDispose();
        }
        [UnityTest] public IEnumerator LoadOneMake1000ReferencesLaterDispose()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = TestUtils.ImageURLs[0];

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

        [UnityTest] public IEnumerator KeepReferenceButDisposeFuture_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return KeepReferenceButDisposeFuture();
        }
        [UnityTest] public IEnumerator KeepReferenceButDisposeFuture()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = TestUtils.ImageURLs[0];

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

        [UnityTest] public IEnumerator LoadOneMake1000ReferencesImmediateDispose_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadOneMake1000ReferencesImmediateDispose();
        }
        [UnityTest] public IEnumerator LoadOneMake1000ReferencesImmediateDispose()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = TestUtils.ImageURLs[0];

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

        [UnityTest] public IEnumerator LoadOneMake1000ReferencesInParallelLateDispose_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadOneMake1000ReferencesInParallelLateDispose();
        }
        [UnityTest] public IEnumerator LoadOneMake1000ReferencesInParallelLateDispose()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = TestUtils.ImageURLs[0];

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

        [UnityTest] public IEnumerator Load1Sprite2TimesAnd2TimesFromCache_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return Load1Sprite2TimesAnd2TimesFromCache();
        }
        [UnityTest] public IEnumerator Load1Sprite2TimesAnd2TimesFromCache()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = TestUtils.ImageURLs[0];

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

        [UnityTest] public IEnumerator Load2SpritesTimesAnd2TimesFromCache_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return Load2SpritesTimesAnd2TimesFromCache();
        }
        [UnityTest] public IEnumerator Load2SpritesTimesAnd2TimesFromCache()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = TestUtils.ImageURLs[0];
            var url2 = TestUtils.ImageURLs[1];

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