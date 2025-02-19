using NUnit.Framework;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine;
using System;
using Extensions.Unity.ImageLoader.Tests.Utils;

namespace Extensions.Unity.ImageLoader.Tests
{
    public class TestFutureInLine : Test
    {
        [UnitySetUp] public override IEnumerator SetUp() => base.SetUp();
        [UnityTearDown] public override IEnumerator TearDown() => base.TearDown();

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
                var future0 = ImageLoader.LoadSprite(url);
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

                future0.Dispose();
                future1.Dispose();
            }
        }
        [UnityTest] public IEnumerator EventLoadedFromMemoryCacheNotCalledBecauseOfCancel_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventLoadedFromMemoryCacheNotCalledBecauseOfCancel();
        }
        [UnityTest] public IEnumerator EventLoadedFromMemoryCacheNotCalledBecauseOfCancel()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            foreach (var url in TestUtils.ImageURLs)
            {
                var sprite = default(Sprite);
                var startTime = DateTime.Now;
                var future0 = ImageLoader.LoadSprite(url);
                var future1 = ImageLoader.LoadSprite(url)
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

                future0.Dispose();
                future1.Dispose();
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
                var future0 = ImageLoader.LoadSprite(url);
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

                future0.Dispose();
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
                var future0 = ImageLoader.LoadSprite(url);
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

                future0.Dispose();
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
                var future0 = ImageLoader.LoadSprite(url);
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

                future0.Dispose();
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
                var future0 = ImageLoader.LoadSprite(url);
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
                future0.Dispose();
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
                var future0 = ImageLoader.LoadSprite(url);
                var future1 = ImageLoader.LoadSprite(url)
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
                future0.Dispose();
                future1.Dispose();
            }
        }
        [UnityTest] public IEnumerator EventLoadingFromDiskCacheThenCancel_CalledImmediately_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventLoadingFromDiskCacheThenCancel_CalledImmediately();
        }
        [UnityTest] public IEnumerator EventLoadingFromDiskCacheThenCancel_CalledImmediately()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = false;

            foreach (var url in TestUtils.ImageURLs)
            {
                yield return ImageLoader.LoadSprite(url).AsCoroutine();
                yield return TestUtils.LoadAndCancel(url, FutureLoadingFrom.DiskCache);
            }
        }
        [UnityTest] public IEnumerator EventLoadingFromSourceCalled_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventLoadingFromSourceCalled();
        }
        [UnityTest] public IEnumerator EventLoadingFromSourceCalled()
        {
            ImageLoader.settings.useDiskCache = false;
            ImageLoader.settings.useMemoryCache = false;

            foreach (var url in TestUtils.ImageURLs)
            {
                var called = false;
                var startTime = DateTime.Now;
                var future0 = ImageLoader.LoadSprite(url);
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
                future0.Dispose();
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
            ImageLoader.settings.useDiskCache = false;
            ImageLoader.settings.useMemoryCache = false;

            foreach (var url in TestUtils.ImageURLs)
            {
                var future0 = ImageLoader.LoadSprite(url);
                var futureListener = future0.ToFutureListener();
                yield return TestUtils.LoadAndCancel(url, FutureLoadingFrom.Source);
                yield return future0.AsCoroutine();
                futureListener.Assert_Events_Equals(EventName.LoadingFromSource, EventName.LoadedFromSource, EventName.Then, EventName.Completed);
                futureListener.Assert_Events_Value<bool>(EventName.Completed, value => value == true);

                future0.ToFutureListener()
                    .Assert_Events_Equals(EventName.LoadingFromSource, EventName.LoadedFromSource, EventName.Then, EventName.Completed)
                    .Assert_Events_Value<bool>(EventName.Completed, value => value == true);

                future0.ToFutureListener(ignoreLoadingWhenLoaded: true)
                    .Assert_Events_Equals(EventName.LoadedFromSource, EventName.Then, EventName.Completed)
                    .Assert_Events_Value<bool>(EventName.Completed, value => value == true);

                future0.Dispose();
            }
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
            var future0 = ImageLoader.LoadSprite(url);
            var future1 = ImageLoader.LoadSprite(url)
                .Timeout(TimeSpan.FromSeconds(0.5f))
                .Failed(e => exception = e);

            Assert.IsNull(exception);

            LogAssert.ignoreFailingMessages = true;
            yield return UniTask.Delay(TimeSpan.FromSeconds(2)).ToCoroutine();
            var task1 = future1.AsTask();
            Assert.IsTrue(task1.IsCompleted);
            Assert.IsNotNull(exception);
            //yield return UniTask.Delay(TimeSpan.FromSeconds(2)).ToCoroutine();
            future0.Dispose();
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
            var future0 = ImageLoader.LoadSprite(url);
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
            future0.Dispose();
            future1.Dispose();
        }
    }
}