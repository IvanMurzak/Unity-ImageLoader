using NUnit.Framework;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine;
using System;

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
        static string IncorrectImageURL => $"https://doesntexist.com/{Guid.NewGuid()}.png";

        [SetUp]
        public void SetUp()
        {
            LogAssert.ignoreFailingMessages = false;
            ImageLoader.settings.debugLevel = DebugLevel.Log;
            ImageLoader.ClearRef();
        }

        [UnityTest] public IEnumerator GetAllLoadingFutures()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var loadingFutures = ImageLoader.GetLoadingFutures();
            Debug.Log($"Loading future count={loadingFutures.Count}");
            foreach (var loadingFuture in loadingFutures)
            {
                Debug.Log($"Loading future: {loadingFuture.Url}, Status={loadingFuture.Status}");
            }
            Assert.Zero(loadingFutures.Count);
        }

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
            var startTime = DateTime.Now;

            var future1 = ImageLoader.LoadSpriteRef(url1);
            var future2 = ImageLoader.LoadSpriteRef(url1);

            LogAssert.ignoreFailingMessages = true;
            future1.Cancel();

            var task2 = future2.AsTask();
            while (!task2.IsCompleted)
            {
                Assert.Less(DateTime.Now - startTime, TimeSpan.FromSeconds(20));
                yield return null;
            }

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
            yield return UniTask.Delay(1000).ToCoroutine();
            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));
        }

        [UnityTest] public IEnumerator DisposeOnOutDisposingBlock()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in ImageURLs)
            {
                using (var future1 = ImageLoader.LoadSpriteRef(url))
                {
                    var task1 = future1.AsTask();
                    Assert.AreEqual(0, Reference<Sprite>.Counter(url));
                    while (!task1.IsCompleted)
                        yield return null;

                    Assert.AreEqual(1, Reference<Sprite>.Counter(url));
                }
                Assert.AreEqual(0, Reference<Sprite>.Counter(url));
            }
            foreach (var url in ImageURLs)
            {
                Assert.AreEqual(0, Reference<Sprite>.Counter(url), $"Should be zero references to URL={url}");
            }
        }
        [UnityTest] public IEnumerator DisposeOnOutDisposingBlock2()
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
            }
            foreach (var url in ImageURLs)
            {
                Assert.AreEqual(0, Reference<Sprite>.Counter(url), $"Should be zero references to URL={url}");
            }
        }
        [UnityTest] public IEnumerator DisposeOnOutDisposingBlock3()
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
                Assert.AreEqual(0, Reference<Sprite>.Counter(url), $"Should be zero references to URL={url}");
            }
        }
        [UnityTest] public IEnumerator EventLoadedFromMemoryCacheCalled()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in ImageURLs)
            {
                var sprite = default(Sprite);
                var startTime = DateTime.Now;
                using var future1 = ImageLoader.LoadSprite(url)
                    .LoadedFromMemoryCache(x => sprite = x);

                var task1 = future1.AsTask();

                while (sprite != null)
                {
                    Assert.Less(DateTime.Now - startTime, TimeSpan.FromSeconds(2));
                    yield return null;
                }

                while (!task1.IsCompleted)
                    yield return null;
            }
        }
        [UnityTest] public IEnumerator EventLoadedFromMemoryCacheNotCalledBecauseOfCancel()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in ImageURLs)
            {
                var sprite = default(Sprite);
                var startTime = DateTime.Now;
                using var future1 = ImageLoader.LoadSprite(url)
                    .LoadedFromMemoryCache(x => sprite = x);

                var task1 = future1.AsTask();
                future1.Cancel();

                while (!task1.IsCompleted)
                {
                    Assert.Less(DateTime.Now - startTime, TimeSpan.FromSeconds(2));
                    yield return null;
                }
                yield return UniTask.Delay(1000).ToCoroutine();
                Assert.IsNull(sprite);
            }
        }
                [UnityTest] public IEnumerator EventLoadedFromDiskCalled()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in ImageURLs)
            {
                var sprite = default(Sprite);
                var startTime = DateTime.Now;
                using var future1 = ImageLoader.LoadSprite(url)
                    .LoadedFromDiskCache(x => sprite = x);

                var task1 = future1.AsTask();

                while (sprite != null)
                {
                    Assert.Less(DateTime.Now - startTime, TimeSpan.FromSeconds(2));
                    yield return null;
                }

                while (!task1.IsCompleted)
                    yield return null;
            }
        }
        [UnityTest] public IEnumerator EventLoadedFromDiskNotCalledBecauseOfCancel()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in ImageURLs)
            {
                var sprite = default(Sprite);
                var startTime = DateTime.Now;
                using var future1 = ImageLoader.LoadSprite(url)
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
            }
        }
        [UnityTest] public IEnumerator EventLoadedFromSourceCalled()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in ImageURLs)
            {
                var sprite = default(Sprite);
                var startTime = DateTime.Now;
                using var future1 = ImageLoader.LoadSprite(url)
                    .LoadedFromSource(x => sprite = x);

                var task1 = future1.AsTask();

                while (sprite != null)
                {
                    Assert.Less(DateTime.Now - startTime, TimeSpan.FromSeconds(2));
                    yield return null;
                }

                while (!task1.IsCompleted)
                    yield return null;
            }
        }
        [UnityTest] public IEnumerator EventLoadedFromSourceNotCalledBecauseOfCancel()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in ImageURLs)
            {
                var sprite = default(Sprite);
                var startTime = DateTime.Now;
                using var future1 = ImageLoader.LoadSprite(url)
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
            }
        }
        [UnityTest] public IEnumerator EventLoadingFromDiskCacheCalled()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = false;

            foreach (var url in ImageURLs)
            {
                yield return ImageLoader.LoadSprite(url).AsUniTask().ToCoroutine();
                var called = false;
                var startTime = DateTime.Now;
                using var future1 = ImageLoader.LoadSprite(url)
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
            }
        }
        [UnityTest] public IEnumerator EventLoadingFromDiskCacheCalledImmediately()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = false;

            foreach (var url in ImageURLs)
            {
                yield return ImageLoader.LoadSprite(url).AsUniTask().ToCoroutine();
                var called = false;
                var startTime = DateTime.Now;
                using var future1 = ImageLoader.LoadSprite(url)
                    .LoadingFromDiskCache(() => called = true);

                Assert.IsTrue(called);

                var task1 = future1.AsTask();
                future1.Cancel();

                Assert.IsTrue(called);
                while (!task1.IsCompleted)
                {
                    Assert.Less(DateTime.Now - startTime, TimeSpan.FromSeconds(2));
                    yield return null;
                }
                Assert.IsTrue(called);
                yield return UniTask.Delay(1000).ToCoroutine();
                Assert.IsTrue(called);
            }
        }
        [UnityTest] public IEnumerator EventLoadingFromSourceCalled()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in ImageURLs)
            {
                var called = false;
                var startTime = DateTime.Now;
                using var future1 = ImageLoader.LoadSprite(url)
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
            }
        }
        [UnityTest] public IEnumerator EventLoadingFromSourceCalledImmediately()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in ImageURLs)
            {
                var called = false;
                var startTime = DateTime.Now;
                using var future1 = ImageLoader.LoadSprite(url)
                    .LoadingFromSource(() => called = true);

                Assert.IsTrue(called);

                var task1 = future1.AsTask();
                future1.Cancel();

                while (!task1.IsCompleted)
                {
                    Assert.Less(DateTime.Now - startTime, TimeSpan.FromSeconds(2));
                    yield return null;
                }
                yield return UniTask.Delay(1000).ToCoroutine();
                Assert.IsTrue(called);
            }
        }
        [UnityTest] public IEnumerator EventFailedWithIncorrectUrlAndTimeout()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = IncorrectImageURL;
            var exception = default(Exception);
            var startTime = DateTime.Now;
            using var future1 = ImageLoader.LoadSprite(url)
                .Timeout(TimeSpan.FromSeconds(0.5f))
                .Failed(e => exception = e);

            Assert.IsNull(exception);

            LogAssert.ignoreFailingMessages = true;
            yield return UniTask.Delay(TimeSpan.FromSeconds(2)).ToCoroutine();
            var task1 = future1.AsTask();
            Assert.IsTrue(task1.IsCompleted);
            Assert.IsNotNull(exception);
            future1.Cancel();
            LogAssert.ignoreFailingMessages = false;
            yield return UniTask.Delay(TimeSpan.FromSeconds(2)).ToCoroutine();
        }
        [UnityTest] public IEnumerator EventFailedWithIncorrectUrlNotCalledBecauseOfCancel()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = IncorrectImageURL;
            var exception = default(Exception);
            var startTime = DateTime.Now;
            using var future1 = ImageLoader.LoadSprite(url)
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
        }
        [UnityTest] public IEnumerator AsyncOperationCompletion()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in ImageURLs)
            {
                var completed = false;
                yield return ImageLoader.LoadSprite(url)
                    .Completed(success => completed = true)
                    .AsUniTask().ToCoroutine();

                Assert.IsTrue(completed);
            }
            yield return UniTask.Delay(TimeSpan.FromSeconds(1)).ToCoroutine();
        }
        [UnityTest] public IEnumerator AsyncOperationCompletionAfterCancel()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;
            LogAssert.ignoreFailingMessages = true;

            foreach (var url in ImageURLs)
            {
                var completed = false;
                var cancelled = false;
                var startTime = DateTime.Now;
                using var future = ImageLoader.LoadSprite(url)
                    .Completed(success => completed = true)
                    .Canceled(() => cancelled = true);

                Assert.IsFalse(completed);
                Assert.IsFalse(cancelled);
                var task1 = future.AsTask();
                future.Cancel();
                var task2 = future.AsTask();

                Assert.IsFalse(completed);
                Assert.IsTrue(cancelled);

                while (!task1.IsCompleted)
                {
                    Assert.Less(DateTime.Now - startTime, TimeSpan.FromSeconds(2));
                    yield return null;
                }
                while (!task2.IsCompleted)
                {
                    Assert.Less(DateTime.Now - startTime, TimeSpan.FromSeconds(2));
                    yield return null;
                }

                Assert.IsFalse(completed);
                Assert.IsTrue(cancelled);
                yield return UniTask.Delay(TimeSpan.FromSeconds(1)).ToCoroutine();
            }
        }
    }
}