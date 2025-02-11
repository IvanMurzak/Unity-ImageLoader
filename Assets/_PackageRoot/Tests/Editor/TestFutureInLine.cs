using NUnit.Framework;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine;
using System;

namespace Extensions.Unity.ImageLoader.Tests
{
    public class TestFutureInLine
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

        [UnityTest] public IEnumerator EventLoadedFromMemoryCacheCalledNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventLoadedFromMemoryCacheCalled();
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
                using var future0 = ImageLoader.LoadSprite(url);
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
        [UnityTest] public IEnumerator EventLoadedFromMemoryCacheNotCalledBecauseOfCancelNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventLoadedFromMemoryCacheNotCalledBecauseOfCancel();
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
                using var future0 = ImageLoader.LoadSprite(url);
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
        [UnityTest] public IEnumerator EventLoadedFromDiskCalledNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventLoadedFromDiskCalled();
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
                using var future0 = ImageLoader.LoadSprite(url);
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
        [UnityTest] public IEnumerator EventLoadedFromDiskNotCalledBecauseOfCancelNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventLoadedFromDiskNotCalledBecauseOfCancel();
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
                using var future0 = ImageLoader.LoadSprite(url);
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
        [UnityTest] public IEnumerator EventLoadedFromSourceCalledNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventLoadedFromSourceCalled();
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
                using var future0 = ImageLoader.LoadSprite(url);
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
        [UnityTest] public IEnumerator EventLoadedFromSourceNotCalledBecauseOfCancelNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventLoadedFromSourceNotCalledBecauseOfCancel();
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
                using var future0 = ImageLoader.LoadSprite(url);
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
        [UnityTest] public IEnumerator EventLoadingFromDiskCacheCalledNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventLoadingFromDiskCacheCalled();
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
                using var future0 = ImageLoader.LoadSprite(url);
                using var future1 = ImageLoader.LoadSprite(url)
                    .LoadingFromDiskCache(() => called = true);

                Assert.AreEqual(future1.Status, FutureStatus.LoadingFromDiskCache);
                Assert.IsTrue(called);
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
        [UnityTest] public IEnumerator EventLoadingFromDiskCacheCalledImmediatelyNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventLoadingFromDiskCacheCalledImmediately();
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
                using var future0 = ImageLoader.LoadSprite(url);
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
        [UnityTest] public IEnumerator EventLoadingFromSourceCalledNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventLoadingFromSourceCalled();
        }
        [UnityTest] public IEnumerator EventLoadingFromSourceCalled()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = false;
            ImageLoader.settings.useMemoryCache = false;

            foreach (var url in ImageURLs)
            {
                var called = false;
                var startTime = DateTime.Now;
                using var future0 = ImageLoader.LoadSprite(url);
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

        [UnityTest] public IEnumerator EventLoadingFromSourceCalledImmediatelyNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventLoadingFromSourceCalledImmediately();
        }
        [UnityTest] public IEnumerator EventLoadingFromSourceCalledImmediately()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = false;
            ImageLoader.settings.useMemoryCache = false;

            foreach (var url in ImageURLs)
            {
                var called = false;
                var startTime = DateTime.Now;
                using var future0 = ImageLoader.LoadSprite(url);
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
        [UnityTest] public IEnumerator EventFailedWithIncorrectUrlAndTimeoutNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventFailedWithIncorrectUrlAndTimeout();
        }
        [UnityTest] public IEnumerator EventFailedWithIncorrectUrlAndTimeout()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = IncorrectImageURL;
            var exception = default(Exception);
            var startTime = DateTime.Now;
            using var future0 = ImageLoader.LoadSprite(url);
            using var future1 = ImageLoader.LoadSprite(url)
                .Timeout(TimeSpan.FromSeconds(0.5f))
                .Failed(e => exception = e);

            Assert.IsNull(exception);

            LogAssert.ignoreFailingMessages = true;
            yield return UniTask.Delay(TimeSpan.FromSeconds(2)).ToCoroutine();
            var task1 = future1.AsTask();
            Assert.IsTrue(task1.IsCompleted);
            Assert.IsNotNull(exception);
            yield return UniTask.Delay(TimeSpan.FromSeconds(2)).ToCoroutine();
        }
        [UnityTest] public IEnumerator EventFailedWithIncorrectUrlNotCalledBecauseOfCancelNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventFailedWithIncorrectUrlNotCalledBecauseOfCancel();
        }
        [UnityTest] public IEnumerator EventFailedWithIncorrectUrlNotCalledBecauseOfCancel()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = IncorrectImageURL;
            var exception = default(Exception);
            var startTime = DateTime.Now;
            using var future0 = ImageLoader.LoadSprite(url);
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
    }
}