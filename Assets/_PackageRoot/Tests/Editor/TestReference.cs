using System;
using System.Linq;
using System.Collections;
using NUnit.Framework;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using UnityEngine;
using System.Threading.Tasks;
using Extensions.Unity.ImageLoader.Tests.Utils;

namespace Extensions.Unity.ImageLoader.Tests
{
    public class TestReference : Test
    {
        [UnitySetUp] public override IEnumerator SetUp() => base.SetUp();
        [UnityTearDown] public override IEnumerator TearDown() => base.TearDown();

        [UnityTest] public IEnumerator CleanMemoryCache_NoLogs() => TestUtils.RunNoLogs(CleanMemoryCache);
        [UnityTest] public IEnumerator CleanMemoryCache()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = TestUtils.ImageURLs[0];

            var task1 = ImageLoader.LoadSpriteRef(url).AsTask();
            yield return task1.TimeoutCoroutine(TimeSpan.FromSeconds(25));

            var ref0 = task1.Result;
            Assert.AreEqual(1, Reference<Sprite>.Counter(url));

            Assert.Throws<Exception>(() => ImageLoader.ClearMemoryCache(url));
            Assert.IsNotNull(ref0.Value);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url));

            ref0.Dispose();
            Assert.IsNull(ref0.Value);
            Assert.AreEqual(0, Reference<Sprite>.Counter(url));
        }

        [UnityTest] public IEnumerator DisposeOnOutOfScope_NoLogs() => TestUtils.RunNoLogs(DisposeOnOutOfScope);
        [UnityTest] public IEnumerator DisposeOnOutOfScope()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = TestUtils.ImageURLs[0];
            { // scope
                var task = ImageLoader.LoadSpriteRef(url).AsTask();
                yield return task.TimeoutCoroutine(TimeSpan.FromSeconds(25));

                var reference = task.Result;
                Assert.NotNull(reference);
                Assert.AreEqual(1, Reference<Sprite>.Counter(url));
            } // end of scope

            yield return TestUtils.WaitForGC();
            yield return TestUtils.WaitForGC();
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
                yield return UniTask.Yield();

            var reference = futureRef.Value;
            Assert.NotNull(reference);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url));

            futureRef = null;
            reference = null;

            yield return TestUtils.WaitForGC();
            yield return TestUtils.WaitForGC();
            yield return TestUtils.WaitForGC();
            Assert.AreEqual(0, Reference<Sprite>.Counter(url));
        }

        [UnityTest] public IEnumerator DisposeOnOutOfScopeAll_NoLogs() => TestUtils.RunNoLogs(DisposeOnOutOfScopeAll);
        [UnityTest] public IEnumerator DisposeOnOutOfScopeAll()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
            {
                var future = ImageLoader.LoadSpriteRef(url);
                yield return future.TimeoutCoroutine(TimeSpan.FromSeconds(10));

                Assert.True(future.IsLoaded);
                Assert.True(future.IsCompleted);
                var reference = future.Value;
                Assert.NotNull(reference);
                Assert.NotNull(reference.Value);
                Assert.AreEqual(1, Reference<Sprite>.Counter(url));
            }

            yield return TestUtils.WaitForGC();
            yield return TestUtils.WaitForGC();
            yield return TestUtils.WaitForGC();

            foreach (var url in TestUtils.ImageURLs)
                Assert.AreEqual(0, Reference<Sprite>.Counter(url));
        }
        [UnityTest] public IEnumerator DisposeOnOutDisposingBlock_NoLogs() => TestUtils.RunNoLogs(DisposeOnOutDisposingBlock);
        [UnityTest] public IEnumerator DisposeOnOutDisposingBlock()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
            {
                var future = ImageLoader.LoadSpriteRef(url);
                yield return future.TimeoutCoroutine(TimeSpan.FromSeconds(10));

                using (var reference = future.Value)
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

        [UnityTest] public IEnumerator CleanMemoryCacheAll_NoLogs() => TestUtils.RunNoLogs(CleanMemoryCacheAll);
        [UnityTest] public IEnumerator CleanMemoryCacheAll()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = TestUtils.ImageURLs[0];

            var future = ImageLoader.LoadSpriteRef(url);
            yield return future.TimeoutCoroutine(TimeSpan.FromSeconds(10));

            var ref0 = future.Value;
            Assert.AreEqual(1, Reference<Sprite>.Counter(url));

            LogAssert.Expect(LogType.Error, $"[ImageLoader] There are 1 references to the object, clear them first. URL={url}");
            ImageLoader.ClearMemoryCacheAll();

            Assert.IsNotNull(ref0.Value);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url));

            ref0.Dispose();
            Assert.IsNull(ref0.Value);
            Assert.AreEqual(0, Reference<Sprite>.Counter(url));
        }

        [UnityTest] public IEnumerator LoadOneMake1000ReferencesLaterDispose_NoLogs() => TestUtils.RunNoLogs(LoadOneMake1000ReferencesLaterDispose);
        [UnityTest] public IEnumerator LoadOneMake1000ReferencesLaterDispose()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = TestUtils.ImageURLs[0];

            var task1 = ImageLoader.LoadSpriteRef(url).AsTask();
            yield return task1.TimeoutCoroutine(TimeSpan.FromSeconds(25));

            var ref1_1 = task1.Result;
            Assert.NotNull(ref1_1);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url));

            var count = 1000;
            var references = new Reference<Sprite>[count];
            for (var i = 0; i < count; i++)
            {
                var reference = ImageLoader.LoadSpriteRefFromMemoryCache(url);
                Assert.NotNull(reference);
                Assert.AreEqual(i + 2, Reference<Sprite>.Counter(url));
                references[i] = reference;
            }

            ref1_1.Dispose();
            Assert.AreEqual(count, Reference<Sprite>.Counter(url));

            for (var i = 0; i < count; i++)
            {
                references[i].Dispose();
                Assert.AreEqual(count - i - 1, Reference<Sprite>.Counter(url));
            }
        }

        [UnityTest] public IEnumerator KeepReferenceButDisposeFuture_NoLogs() => TestUtils.RunNoLogs(KeepReferenceButDisposeFuture);
        [UnityTest] public IEnumerator KeepReferenceButDisposeFuture()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = TestUtils.ImageURLs[0];

            var future = ImageLoader.LoadSpriteRef(url);
            var task1 = future.AsTask();
            yield return task1.TimeoutCoroutine(TimeSpan.FromSeconds(25));

            Assert.AreEqual(1, Reference<Sprite>.Counter(url));

            var ref1 = task1.Result;
            Assert.NotNull(ref1);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url));

            future.Dispose();
            Assert.AreEqual(1, Reference<Sprite>.Counter(url));

            ref1.Dispose();
            Assert.AreEqual(0, Reference<Sprite>.Counter(url));
        }

        [UnityTest] public IEnumerator LoadOneMake1000ReferencesImmediateDispose_NoLogs() => TestUtils.RunNoLogs(LoadOneMake1000ReferencesImmediateDispose);
        [UnityTest] public IEnumerator LoadOneMake1000ReferencesImmediateDispose()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = TestUtils.ImageURLs[0];

            var future = ImageLoader.LoadSpriteRef(url);
            var task1 = future.AsTask();
            yield return task1.TimeoutCoroutine(TimeSpan.FromSeconds(25));

            var ref1_1 = task1.Result;
            Assert.NotNull(ref1_1);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url));

            var count = 1000;
            for (var i = 0; i < count; i++)
            {
                Assert.AreEqual(1, Reference<Sprite>.Counter(url), $"ref[{i}] going to create.");

                var reference = ImageLoader.LoadSpriteRefFromMemoryCache(url);
                Assert.NotNull(reference);
                Assert.AreEqual(2, Reference<Sprite>.Counter(url), $"ref[{i}] is created, but reference counter is wrong.");

                reference.Dispose();
                Assert.AreEqual(1, Reference<Sprite>.Counter(url), $"ref[{i}] is disposed, but it should be still in memory cache because of another ref.");
            }
            Assert.AreEqual(1, Reference<Sprite>.Counter(url));

            future.Dispose();
            Assert.AreEqual(1, Reference<Sprite>.Counter(url));

            ref1_1.Dispose();
            Assert.AreEqual(0, Reference<Sprite>.Counter(url));
        }

        [UnityTest] public IEnumerator LoadOneMake1000ReferencesInParallelLateDispose_NoLogs() => TestUtils.RunNoLogs(LoadOneMake1000ReferencesInParallelLateDispose);
        [UnityTest] public IEnumerator LoadOneMake1000ReferencesInParallelLateDispose()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = TestUtils.ImageURLs[0];

            var task1 = ImageLoader.LoadSpriteRef(url).AsTask();
            yield return task1.TimeoutCoroutine(TimeSpan.FromSeconds(25));

            var ref1_1 = task1.Result;
            Assert.NotNull(ref1_1);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url));

            var count = 1000;
            var references = new Reference<Sprite>[count];
            var tasks = Enumerable.Range(0, count)
                .Select(i => Task.Run(() =>
                {
                    var reference = ImageLoader.LoadSpriteRefFromMemoryCache(url);
                    Assert.NotNull(reference);
                    references[i] = reference;
                    return reference;
                }))
                .ToArray();

            yield return Task.WhenAll(tasks).TimeoutCoroutine(TimeSpan.FromSeconds(10));
            Assert.AreEqual(count + 1, Reference<Sprite>.Counter(url));

            foreach (var reference in references)
                reference.Dispose();
            Assert.AreEqual(1, Reference<Sprite>.Counter(url));

            ref1_1.Dispose();
            Assert.AreEqual(0, Reference<Sprite>.Counter(url));
        }

        [UnityTest] public IEnumerator Load1Sprite2TimesAnd2TimesFromCache_NoLogs() => TestUtils.RunNoLogs(Load1Sprite2TimesAnd2TimesFromCache);
        [UnityTest] public IEnumerator Load1Sprite2TimesAnd2TimesFromCache()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = TestUtils.ImageURLs[0];

            var task1 = ImageLoader.LoadSpriteRef(url).AsTask();
            yield return task1.TimeoutCoroutine(TimeSpan.FromSeconds(25));

            var ref0 = task1.Result;
            Assert.AreEqual(1, Reference<Sprite>.Counter(url));

            var task2 = ImageLoader.LoadSpriteRef(url).AsTask();
            yield return task2.TimeoutCoroutine(TimeSpan.FromSeconds(25));

            var ref1 = task2.Result;
            Assert.AreEqual(2, Reference<Sprite>.Counter(url));

            var ref2 = ImageLoader.LoadSpriteRefFromMemoryCache(url);
            Assert.AreEqual(3, Reference<Sprite>.Counter(url));

            var ref3 = ImageLoader.LoadSpriteRefFromMemoryCache(url);
            Assert.AreEqual(4, Reference<Sprite>.Counter(url));

            ref0.Dispose();
            Assert.AreEqual(3, Reference<Sprite>.Counter(url));
            Assert.IsNull(ref0.Value);
            Assert.IsNotNull(ref1.Value);
            Assert.IsNotNull(ref2.Value);
            Assert.IsNotNull(ref3.Value);

            ref1.Dispose();
            Assert.AreEqual(2, Reference<Sprite>.Counter(url));
            Assert.IsNull(ref0.Value);
            Assert.IsNull(ref1.Value);
            Assert.IsNotNull(ref2.Value);
            Assert.IsNotNull(ref3.Value);

            ref2.Dispose();
            Assert.AreEqual(1, Reference<Sprite>.Counter(url));
            Assert.IsNull(ref0.Value);
            Assert.IsNull(ref1.Value);
            Assert.IsNull(ref2.Value);
            Assert.IsNotNull(ref3.Value);

            ref3.Dispose();
            Assert.AreEqual(0, Reference<Sprite>.Counter(url));
            Assert.IsNull(ref0.Value);
            Assert.IsNull(ref1.Value);
            Assert.IsNull(ref2.Value);
            Assert.IsNull(ref3.Value);
        }

        [UnityTest] public IEnumerator Load2SpritesTimesAnd2TimesFromCache_NoLogs() => TestUtils.RunNoLogs(Load2SpritesTimesAnd2TimesFromCache);
        [UnityTest] public IEnumerator Load2SpritesTimesAnd2TimesFromCache()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = TestUtils.ImageURLs[0];
            var url2 = TestUtils.ImageURLs[1];

            var task1 = ImageLoader.LoadSpriteRef(url).AsTask();
            yield return task1.TimeoutCoroutine(TimeSpan.FromSeconds(25));

            var ref1_1 = task1.Result;
            Assert.NotNull(ref1_1);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url));

            var task2 = ImageLoader.LoadSpriteRef(url2).AsTask();
            yield return task2.TimeoutCoroutine(TimeSpan.FromSeconds(25));

            var ref2_1 = task2.Result;
            Assert.NotNull(ref2_1);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url2));

            var ref1_2 = ImageLoader.LoadSpriteRefFromMemoryCache(url);
            Assert.NotNull(ref1_2);
            Assert.AreEqual(2, Reference<Sprite>.Counter(url));

            var ref2_2 = ImageLoader.LoadSpriteRefFromMemoryCache(url2);
            Assert.NotNull(ref2_2);
            Assert.AreEqual(2, Reference<Sprite>.Counter(url2));

            ref1_1.Dispose();
            Assert.AreEqual(1, Reference<Sprite>.Counter(url));
            Assert.AreEqual(2, Reference<Sprite>.Counter(url2));
            Assert.IsNull(ref1_1.Value);
            Assert.IsNotNull(ref2_1.Value);
            Assert.IsNotNull(ref1_2.Value);
            Assert.IsNotNull(ref2_2.Value);

            ref2_1.Dispose();
            Assert.AreEqual(1, Reference<Sprite>.Counter(url));
            Assert.AreEqual(1, Reference<Sprite>.Counter(url2));
            Assert.IsNull(ref1_1.Value);
            Assert.IsNull(ref2_1.Value);
            Assert.IsNotNull(ref1_2.Value);
            Assert.IsNotNull(ref2_2.Value);

            ref1_2.Dispose();
            Assert.AreEqual(0, Reference<Sprite>.Counter(url));
            Assert.AreEqual(1, Reference<Sprite>.Counter(url2));
            Assert.IsNull(ref1_1.Value);
            Assert.IsNull(ref2_1.Value);
            Assert.IsNull(ref1_2.Value);
            Assert.IsNotNull(ref2_2.Value);

            ref2_2.Dispose();
            Assert.AreEqual(0, Reference<Sprite>.Counter(url));
            Assert.AreEqual(0, Reference<Sprite>.Counter(url2));
            Assert.IsNull(ref1_1.Value);
            Assert.IsNull(ref2_1.Value);
            Assert.IsNull(ref1_2.Value);
            Assert.IsNull(ref2_2.Value);
        }
    }
}