using NUnit.Framework;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine;
using System;
using Extensions.Unity.ImageLoader.Tests.Utils;
using System.Threading.Tasks;

namespace Extensions.Unity.ImageLoader.Tests
{
    public partial class TestFuture : Test
    {
        [UnitySetUp] public override IEnumerator SetUp() => base.SetUp();
        [UnityTearDown] public override IEnumerator TearDown() => base.TearDown();

        [UnityTest] public IEnumerator GetAllLoadingFutures_NoLogs() => TestUtils.RunNoLogs(GetAllLoadingFutures);
        [UnityTest] public IEnumerator GetAllLoadingFutures()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var loadingSpriteFutures = ImageLoader.GetLoadingSpriteFutures();
            Assert.NotNull(loadingSpriteFutures);
            Debug.Log($"Loading Future<Sprite> count={loadingSpriteFutures.Count}");
            foreach (var loadingFuture in loadingSpriteFutures)
            {
                Debug.Log($"Loading Future<Sprite>: {loadingFuture.Url}, Status={loadingFuture.Status}");
            }
            Assert.Zero(loadingSpriteFutures.Count);
            yield return UniTask.Yield();

            var loadingTextureFutures = ImageLoader.GetLoadingTextureFutures();
            Assert.NotNull(loadingTextureFutures);
            Debug.Log($"Loading Future<Texture2D> count={loadingTextureFutures.Count}");
            foreach (var loadingFuture in loadingTextureFutures)
            {
                Debug.Log($"Loading Future<Texture2D>: {loadingFuture.Url}, Status={loadingFuture.Status}");
            }
            Assert.Zero(loadingTextureFutures.Count);
        }
        [UnityTest] public IEnumerator LoadingRefAndWaiting_NoLogs() => TestUtils.RunNoLogs(LoadingRefAndWaiting);
        [UnityTest] public IEnumerator LoadingRefAndWaiting()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = TestUtils.ImageURLs[0];

            var task1 = ImageLoader.LoadSpriteRef(url).AsTask();
            yield return task1.TimeoutCoroutine(TimeSpan.FromSeconds(25));

            var ref0 = task1.Result;
            Assert.IsNotNull(ref0);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url));

            Assert.Throws<Exception>(() => ImageLoader.ClearMemoryCache(url));

            ref0.Dispose();
            Assert.IsNull(ref0.Value);
            Assert.AreEqual(0, Reference<Sprite>.Counter(url));
        }
        [UnityTest] public IEnumerator Loading2RefAndCancelFirst_NoLogs() => TestUtils.RunNoLogs(Loading2RefAndCancelFirst);
        [UnityTest] public IEnumerator Loading2RefAndCancelFirst()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = TestUtils.ImageURLs[0];

            var future1 = ImageLoader.LoadSpriteRef(url);
            var future2 = ImageLoader.LoadSpriteRef(url);

            future1.Cancel();

            var task1 = future2.AsTask();
            yield return task1.TimeoutCoroutine(TimeSpan.FromSeconds(25));

            var ref2 = task1.Result;
            Assert.IsNotNull(ref2);
            Assert.IsNotNull(ref2.Value);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url));
            future1.Dispose();
            future2.Dispose();
            ref2.Dispose();
        }
        [UnityTest] public IEnumerator Loading2RefAndWait_NoLogs() => TestUtils.RunNoLogs(Loading2RefAndWait);
        [UnityTest] public IEnumerator Loading2RefAndWait()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = TestUtils.ImageURLs[0];

            var task1 = ImageLoader.LoadSpriteRef(url).AsTask();
            var task2 = ImageLoader.LoadSpriteRef(url).AsTask();

            yield return Task.WhenAll(task1, task2).TimeoutCoroutine(TimeSpan.FromSeconds(25));

            var ref0 = task1.Result;
            Assert.IsNotNull(ref0);
            var ref1 = task2.Result;
            Assert.IsNotNull(ref1);
            Assert.AreEqual(2, Reference<Sprite>.Counter(url));

            Assert.Throws<Exception>(() => ImageLoader.ClearMemoryCache(url));

            ref0.Dispose();
            Assert.IsNull(ref0.Value);
            Assert.AreEqual(1, Reference<Sprite>.Counter(url));
            ref1.Dispose();
            Assert.IsNull(ref1.Value);
            Assert.AreEqual(0, Reference<Sprite>.Counter(url));

            var sprite = ImageLoader.LoadSpriteFromMemoryCache(url);
            Assert.IsNull(sprite);

            ImageLoader.ClearMemoryCache(url);
            Assert.AreEqual(0, Reference<Sprite>.Counter(url));
        }
        [UnityTest] public IEnumerator Loading2RefAndDisposeAll_NoLogs() => TestUtils.RunNoLogs(Loading2RefAndDisposeAll);
        [UnityTest] public IEnumerator Loading2RefAndDisposeAll()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = TestUtils.ImageURLs[0];

            var future1 = ImageLoader.LoadSpriteRef(url);
            Assert.IsNotNull(future1);
            var future2 = ImageLoader.LoadSpriteRef(url);
            Assert.IsNotNull(future2);

            Assert.AreEqual(0, Reference<Sprite>.Counter(url));

            future1.Cancel();
            future2.Cancel();

            future1.Dispose();
            future2.Dispose();

            Assert.AreEqual(0, Reference<Sprite>.Counter(url));
            yield return TestUtils.Wait(TimeSpan.FromSeconds(1));
            Assert.AreEqual(0, Reference<Sprite>.Counter(url));
        }
        [UnityTest] public IEnumerator DisposeOnOutDisposingBlock_NoLogs() => TestUtils.RunNoLogs(DisposeOnOutDisposingBlock);
        [UnityTest] public IEnumerator DisposeOnOutDisposingBlock()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
            {
                var future1 = ImageLoader.LoadSpriteRef(url);
                var task1 = future1.AsTask();
                Assert.AreEqual(0, Reference<Sprite>.Counter(url));
                yield return task1.TimeoutCoroutine(TimeSpan.FromSeconds(25));

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
        [UnityTest] public IEnumerator DisposeOnOutDisposingBlock2_NoLogs() => TestUtils.RunNoLogs(DisposeOnOutDisposingBlock2);
        [UnityTest] public IEnumerator DisposeOnOutDisposingBlock2()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
            {
                var future1 = ImageLoader.LoadSpriteRef(url);
                var task1 = future1.AsTask();
                yield return task1.TimeoutCoroutine(TimeSpan.FromSeconds(25));

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
        [UnityTest] public IEnumerator DisposeOnOutDisposingBlock3_NoLogs() => TestUtils.RunNoLogs(DisposeOnOutDisposingBlock3);
        [UnityTest] public IEnumerator DisposeOnOutDisposingBlock3()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
            {
                var future1 = ImageLoader.LoadSpriteRef(url);
                var task1 = future1.AsTask();
                yield return task1.TimeoutCoroutine(TimeSpan.FromSeconds(25));

                Assert.AreEqual(1, Reference<Sprite>.Counter(url));

                future1.Value.Dispose();
                Assert.AreEqual(0, Reference<Sprite>.Counter(url));

                var future2 = ImageLoader.LoadSpriteRef(url);
                var task2 = future2.AsTask();
                yield return task2.TimeoutCoroutine(TimeSpan.FromSeconds(25));

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

        [UnityTest] public IEnumerator EventFailedWithIncorrectUrlAndTimeout_NoLogs() => TestUtils.RunNoLogs(EventFailedWithIncorrectUrlAndTimeout);
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
            yield return TestUtils.Wait(TimeSpan.FromSeconds(2));
            var task1 = future1.AsTask();
            Assert.IsTrue(task1.IsCompleted);
            Assert.IsNotNull(exception);

            future1.Cancel(); // expected warning
            LogAssert.ignoreFailingMessages = false;
            yield return TestUtils.Wait(TimeSpan.FromSeconds(2));
            future1.Dispose();
        }
        [UnityTest] public IEnumerator EventFailedWithIncorrectUrlNotCalledBecauseOfCancel_NoLogs() => TestUtils.RunNoLogs(EventFailedWithIncorrectUrlNotCalledBecauseOfCancel);
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

            yield return task1.TimeoutCoroutine(TimeSpan.FromSeconds(25));
            yield return TestUtils.Wait(TimeSpan.FromSeconds(1));
            Assert.IsNull(exception);
            future1.Dispose();
        }
        [UnityTest] public IEnumerator AsyncOperationCompletion_NoLogs() => TestUtils.RunNoLogs(AsyncOperationCompletion);
        [UnityTest] public IEnumerator AsyncOperationCompletion()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
            {
                var completed = false;
                yield return ImageLoader.LoadSprite(url)
                    .Completed(success => completed = true)
                    .TimeoutCoroutine(TimeSpan.FromSeconds(10));

                Assert.IsTrue(completed);
            }
            yield return TestUtils.Wait(TimeSpan.FromSeconds(1));
        }
        [UnityTest] public IEnumerator AsyncOperationCompletionAfterCancel_NoLogs() => TestUtils.RunNoLogs(AsyncOperationCompletionAfterCancel);
        [UnityTest] public IEnumerator AsyncOperationCompletionAfterCancel()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
                yield return TestUtils.LoadAndCancel(url, FutureLoadingFrom.Source);
        }
    }
}