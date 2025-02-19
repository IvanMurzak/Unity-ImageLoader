using NUnit.Framework;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine;
using System;

namespace Extensions.Unity.ImageLoader.Tests
{
    public partial class TestFuture
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

        [UnityTest] public IEnumerator GetAllLoadingFutures_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return GetAllLoadingFutures();
        }
        [UnityTest] public IEnumerator GetAllLoadingFutures()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var loadingFutures = ImageLoader.GetLoadingFutures();
            Assert.NotNull(loadingFutures);
            Debug.Log($"Loading future count={loadingFutures.Count}");
            foreach (var loadingFuture in loadingFutures)
            {
                Debug.Log($"Loading future: {loadingFuture.Url}, Status={loadingFuture.Status}");
            }
            Assert.Zero(loadingFutures.Count);
            yield return null;
        }
        [UnityTest] public IEnumerator LoadingRefAndWaiting_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadingRefAndWaiting();
        }
        [UnityTest] public IEnumerator LoadingRefAndWaiting()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = TestUtils.ImageURLs[0];

            var task1 = ImageLoader.LoadSpriteRef(url1).AsTask();
            while (!task1.IsCompleted)
                yield return null;

            var ref0 = task1.Result;
            Assert.IsNotNull(ref0);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));

            Assert.Throws<Exception>(() => ImageLoader.ClearMemoryCache(url1));

            ref0.Dispose();
            Assert.IsNull(ref0.Value);
            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));
        }
        [UnityTest] public IEnumerator Loading2RefAndCancelFirst_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return Loading2RefAndCancelFirst();
        }
        [UnityTest] public IEnumerator Loading2RefAndCancelFirst()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = TestUtils.ImageURLs[0];
            var startTime = DateTime.Now;

            var future1 = ImageLoader.LoadSpriteRef(url1);
            var future2 = ImageLoader.LoadSpriteRef(url1);

            future1.Cancel();

            var task2 = future2.AsTask();
            while (!task2.IsCompleted)
            {
                Assert.Less(DateTime.Now - startTime, TimeSpan.FromSeconds(25));
                yield return null;
            }

            var ref2 = task2.Result;
            Assert.IsNotNull(ref2);
            Assert.IsNotNull(ref2.Value);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));
            future1.Dispose();
            future2.Dispose();
            ref2.Dispose();
        }
        [UnityTest] public IEnumerator Loading2RefAndWait_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return Loading2RefAndWait();
        }
        [UnityTest] public IEnumerator Loading2RefAndWait()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = TestUtils.ImageURLs[0];

            var task1 = ImageLoader.LoadSpriteRef(url1).AsTask();
            var task2 = ImageLoader.LoadSpriteRef(url1).AsTask();

            while (!task1.IsCompleted || !task2.IsCompleted)
                yield return null;

            var ref0 = task1.Result;
            Assert.IsNotNull(ref0);
            var ref1 = task2.Result;
            Assert.IsNotNull(ref1);
            Assert.AreEqual(2, Reference<Sprite>.Counter(url1));

            Assert.Throws<Exception>(() => ImageLoader.ClearMemoryCache(url1));

            ref0.Dispose();
            Assert.IsNull(ref0.Value);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));
            ref1.Dispose();
            Assert.IsNull(ref1.Value);
            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));

            var sprite = ImageLoader.LoadFromMemoryCache(url1);
            Assert.IsNull(sprite);

            ImageLoader.ClearMemoryCache(url1);
            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));
        }
        [UnityTest] public IEnumerator Loading2RefAndDisposeAll_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return Loading2RefAndDisposeAll();
        }
        [UnityTest] public IEnumerator Loading2RefAndDisposeAll()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = TestUtils.ImageURLs[0];

            var future1 = ImageLoader.LoadSpriteRef(url1);
            Assert.IsNotNull(future1);
            var future2 = ImageLoader.LoadSpriteRef(url1);
            Assert.IsNotNull(future2);

            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));

            future1.Cancel();
            future2.Cancel();

            future1.Dispose();
            future2.Dispose();

            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));
            yield return UniTask.Delay(1000).ToCoroutine();
            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));
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
                var future1 = ImageLoader.LoadSpriteRef(url);
                var task1 = future1.AsTask();
                Assert.AreEqual(0, Reference<Sprite>.Counter(url));
                while (!task1.IsCompleted)
                    yield return null;

                using (var ref1 = task1.Result)
                {
                    Assert.AreEqual(1, Reference<Sprite>.Counter(url));
                }
                Assert.AreEqual(0, Reference<Sprite>.Counter(url));
            }
            foreach (var url in TestUtils.ImageURLs)
            {
                Assert.AreEqual(0, Reference<Sprite>.Counter(url), $"Should be zero references to URL={url}");
            }
        }
        [UnityTest] public IEnumerator DisposeOnOutDisposingBlock2_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return DisposeOnOutDisposingBlock2();
        }
        [UnityTest] public IEnumerator DisposeOnOutDisposingBlock2()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
            {
                var future1 = ImageLoader.LoadSpriteRef(url);
                var task1 = future1.AsTask();
                while (!task1.IsCompleted)
                    yield return null;

                Assert.AreEqual(1, Reference<Sprite>.Counter(url));

                var ref1 = future1.Value;
                ref1.Dispose();
                Assert.AreEqual(0, Reference<Sprite>.Counter(url));
            }
            foreach (var url in TestUtils.ImageURLs)
            {
                Assert.AreEqual(0, Reference<Sprite>.Counter(url), $"Should be zero references to URL={url}");
            }
        }
        [UnityTest] public IEnumerator DisposeOnOutDisposingBlock3_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return DisposeOnOutDisposingBlock3();
        }
        [UnityTest] public IEnumerator DisposeOnOutDisposingBlock3()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
            {
                var future1 = ImageLoader.LoadSpriteRef(url);
                var task1 = future1.AsTask();
                while (!task1.IsCompleted)
                    yield return null;

                Assert.AreEqual(1, Reference<Sprite>.Counter(url));

                future1.Value.Dispose();
                Assert.AreEqual(0, Reference<Sprite>.Counter(url));

                var future2 = ImageLoader.LoadSpriteRef(url);
                var task2 = future2.AsTask();
                while (!task2.IsCompleted)
                    yield return null;

                using (var ref2 = task2.Result)
                {
                    Assert.AreEqual(1, Reference<Sprite>.Counter(url));
                }
                Assert.AreEqual(0, Reference<Sprite>.Counter(url));
            }
            foreach (var url in TestUtils.ImageURLs)
            {
                Assert.AreEqual(0, Reference<Sprite>.Counter(url), $"Should be zero references to URL={url}");
            }
        }
        [UnityTest] public IEnumerator EventLoadedFromMemoryCacheCalled_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventLoadedFromMemoryCacheCalled();
        }
        [UnityTest] public IEnumerator EventLoadedFromMemoryCacheCalled()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
            {
                var sprite = default(Sprite);
                var startTime = DateTime.Now;
                var future1 = ImageLoader.LoadSprite(url)
                    .LoadedFromMemoryCache(x => sprite = x);

                var task1 = future1.AsTask();

                while (sprite != null)
                {
                    Assert.Less(DateTime.Now - startTime, TimeSpan.FromSeconds(2));
                    yield return null;
                }

                while (!task1.IsCompleted)
                    yield return null;

                future1.Dispose();
            }
        }
        [UnityTest] public IEnumerator EventLoadedFromMemoryCache_ThenCancel_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventLoadedFromMemoryCache_ThenCancel();
        }
        [UnityTest] public IEnumerator EventLoadedFromMemoryCache_ThenCancel()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
            {
                yield return ImageLoader.LoadSprite(url).AsCoroutine();
                yield return LoadFromMemoryCacheThenCancel(url);
            }
        }
        [UnityTest] public IEnumerator EventLoadedFromDiskCalled_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventLoadedFromDiskCalled();
        }
        [UnityTest] public IEnumerator EventLoadedFromDiskCalled()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
            {
                var sprite = default(Sprite);
                var startTime = DateTime.Now;
                var future1 = ImageLoader.LoadSprite(url)
                    .LoadedFromDiskCache(x => sprite = x);

                var task1 = future1.AsTask();

                while (sprite != null)
                {
                    Assert.Less(DateTime.Now - startTime, TimeSpan.FromSeconds(2));
                    yield return null;
                }

                while (!task1.IsCompleted)
                    yield return null;

                future1.Dispose();
            }
        }
        [UnityTest] public IEnumerator EventLoadedFromDiskNotCalledBecauseOfCancel_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventLoadedFromDiskNotCalledBecauseOfCancel();
        }
        [UnityTest] public IEnumerator EventLoadedFromDiskNotCalledBecauseOfCancel()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
            {
                var sprite = default(Sprite);
                var startTime = DateTime.Now;
                var future1 = ImageLoader.LoadSprite(url)
                    .LoadedFromDiskCache(x => sprite = x);

                var task1 = future1.AsTask();
                future1.Cancel();

                while (!task1.IsCompleted)
                {
                    Assert.Less(DateTime.Now - startTime, TimeSpan.FromSeconds(2));
                    yield return null;
                }
                yield return UniTask.Delay(1000).ToCoroutine();
                Assert.IsNull(sprite);

                future1.Dispose();
            }
        }
        [UnityTest] public IEnumerator EventLoadedFromSourceCalled_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventLoadedFromSourceCalled();
        }
        [UnityTest] public IEnumerator EventLoadedFromSourceCalled()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
            {
                var sprite = default(Sprite);
                var startTime = DateTime.Now;
                var future1 = ImageLoader.LoadSprite(url)
                    .LoadedFromSource(x => sprite = x);

                var task1 = future1.AsTask();

                while (sprite != null)
                {
                    Assert.Less(DateTime.Now - startTime, TimeSpan.FromSeconds(2));
                    yield return null;
                }

                while (!task1.IsCompleted)
                    yield return null;

                future1.Dispose();
            }
        }
        [UnityTest] public IEnumerator EventLoadedFromSourceNotCalledBecauseOfCancel_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventLoadedFromSourceNotCalledBecauseOfCancel();
        }
        [UnityTest] public IEnumerator EventLoadedFromSourceNotCalledBecauseOfCancel()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
            {
                var sprite = default(Sprite);
                var startTime = DateTime.Now;
                var future1 = ImageLoader.LoadSprite(url)
                    .LoadedFromSource(x => sprite = x);

                var task1 = future1.AsTask();
                future1.Cancel();

                while (!task1.IsCompleted)
                {
                    Assert.Less(DateTime.Now - startTime, TimeSpan.FromSeconds(2));
                    yield return null;
                }
                yield return UniTask.Delay(1000).ToCoroutine();
                Assert.IsNull(sprite);

                future1.Dispose();
            }
        }
        [UnityTest] public IEnumerator EventLoadingFromDiskCacheCalled_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventLoadingFromDiskCacheCalled();
        }
        [UnityTest] public IEnumerator EventLoadingFromDiskCacheCalled()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = false;

            foreach (var url in TestUtils.ImageURLs)
            {
                yield return ImageLoader.LoadSprite(url).AsUniTask().ToCoroutine();
                var called = false;
                var startTime = DateTime.Now;
                var future1 = ImageLoader.LoadSprite(url)
                    .LoadingFromDiskCache(() => called = true);

                var task1 = future1.AsTask();

                while (!called)
                {
                    Assert.Less(DateTime.Now - startTime, TimeSpan.FromSeconds(2));
                    yield return null;
                }

                while (!task1.IsCompleted)
                    yield return null;

                Assert.IsTrue(called);
                future1.Dispose();
            }
        }
        [UnityTest] public IEnumerator EventLoadingFromDiskCacheThenCancelCalledImmediately_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventLoadingFromDiskCacheThenCancelCalledImmediately();
        }
        [UnityTest] public IEnumerator EventLoadingFromDiskCacheThenCancelCalledImmediately()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = false;

            foreach (var url in TestUtils.ImageURLs)
            {
                yield return ImageLoader.LoadSprite(url).AsCoroutine();
                yield return LoadAndCancel(url, FutureLoadingFrom.DiskCache);
            }
        }
        [UnityTest] public IEnumerator EventLoadingFromSourceCalled_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventLoadingFromSourceCalled();
        }
        [UnityTest] public IEnumerator EventLoadingFromSourceCalled()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
            {
                var called = false;
                var startTime = DateTime.Now;
                var future1 = ImageLoader.LoadSprite(url)
                    .LoadingFromSource(() => called = true);

                var task1 = future1.AsTask();

                while (!called)
                {
                    Assert.Less(DateTime.Now - startTime, TimeSpan.FromSeconds(2));
                    yield return null;
                }

                while (!task1.IsCompleted)
                    yield return null;

                Assert.IsTrue(called);
                future1.Dispose();
            }
        }
        [UnityTest] public IEnumerator EventLoadingFromSourceCalledImmediately_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventLoadingFromSourceCalledImmediately();
        }
        [UnityTest] public IEnumerator EventLoadingFromSourceCalledImmediately()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
                yield return LoadAndCancel(url, FutureLoadingFrom.Source);
        }
        [UnityTest] public IEnumerator EventFailedWithIncorrectUrlAndTimeout_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventFailedWithIncorrectUrlAndTimeout();
        }
        [UnityTest] public IEnumerator EventFailedWithIncorrectUrlAndTimeout()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = TestUtils.IncorrectImageURL;
            var exception = default(Exception);
            var startTime = DateTime.Now;
            var future1 = ImageLoader.LoadSprite(url)
                .Timeout(TimeSpan.FromSeconds(0.1f))
                .Failed(e => exception = e);

            Assert.IsNull(exception);

            LogAssert.ignoreFailingMessages = true;
            yield return UniTask.Delay(TimeSpan.FromSeconds(2)).ToCoroutine();
            var task1 = future1.AsTask();
            Assert.IsTrue(task1.IsCompleted);
            Assert.IsNotNull(exception);

            future1.Cancel(); // expected warning
            LogAssert.ignoreFailingMessages = false;
            yield return UniTask.Delay(TimeSpan.FromSeconds(2)).ToCoroutine();
            future1.Dispose();
        }
        [UnityTest] public IEnumerator EventFailedWithIncorrectUrlNotCalledBecauseOfCancel_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventFailedWithIncorrectUrlNotCalledBecauseOfCancel();
        }
        [UnityTest] public IEnumerator EventFailedWithIncorrectUrlNotCalledBecauseOfCancel()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = TestUtils.IncorrectImageURL;
            var exception = default(Exception);
            var startTime = DateTime.Now;
            var future1 = ImageLoader.LoadSprite(url)
                .Failed(e => exception = e);

            Assert.IsNull(exception);

            var task1 = future1.AsTask();
            future1.Cancel();

            while (!task1.IsCompleted)
            {
                Assert.Less(DateTime.Now - startTime, TimeSpan.FromSeconds(2));
                yield return null;
            }
            yield return UniTask.Delay(1000).ToCoroutine();
            Assert.IsNull(exception);
            future1.Dispose();
        }
        [UnityTest] public IEnumerator AsyncOperationCompletion_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return AsyncOperationCompletion();
        }
        [UnityTest] public IEnumerator AsyncOperationCompletion()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
            {
                var completed = false;
                yield return ImageLoader.LoadSprite(url)
                    .Completed(success => completed = true)
                    .AsUniTask().ToCoroutine();

                Assert.IsTrue(completed);
            }
            yield return UniTask.Delay(TimeSpan.FromSeconds(1)).ToCoroutine();
        }
        [UnityTest] public IEnumerator AsyncOperationCompletionAfterCancel_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return AsyncOperationCompletionAfterCancel();
        }
        [UnityTest] public IEnumerator AsyncOperationCompletionAfterCancel()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
                yield return LoadAndCancel(url, FutureLoadingFrom.Source);
        }
    }
}