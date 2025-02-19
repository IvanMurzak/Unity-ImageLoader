using NUnit.Framework;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine;
using System;
using Extensions.Unity.ImageLoader.Tests.Utils;
using System.Collections.Generic;

namespace Extensions.Unity.ImageLoader.Tests
{
    public class TestFutureOrder
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
        [UnityTest] public IEnumerator EventsLoadedWhenClear_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventsLoadedWhenClear();
        }
        [UnityTest] public IEnumerator EventsLoadedWhenClear()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = TestUtils.ImageURLs[0];
            var startTime = DateTime.Now;

            var future = new FutureSprite(url);
            var futureListener = new FutureListener<Sprite>(future);
            var task = future.StartLoading().AsTask();
            yield return new WaitUntil(() => task.IsCompleted);

            futureListener.Assert_Events_Equals(new List<EventName>
            {
                EventName.LoadingFromSource,
                EventName.LoadedFromSource,
                EventName.Then,
                EventName.Completed
            });
            futureListener.Assert_Events_Value(EventName.LoadedFromSource, sprite => sprite != null);
            futureListener.Assert_Events_Value(EventName.Then, sprite => sprite != null);
            futureListener.Assert_Events_Value(EventName.Completed, success => ((bool)success) == true);
            futureListener.Assert_Events_NotContains(EventName.Canceled);

            var task1 = future.AsTask();
            Assert.True(task1.IsCompleted);
            Assert.AreEqual(FutureStatus.LoadedFromSource, future.Status);

            future.Cancel();
            Assert.AreEqual(FutureStatus.LoadedFromSource, future.Status);

            var task2 = future.AsTask();
            Assert.True(task2.IsCompleted);
            Assert.AreEqual(FutureStatus.LoadedFromSource, future.Status);

            future.Dispose();
        }
        [UnityTest] public IEnumerator EventsFailedWhenClear_NoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventsFailedWhenClear();
        }
        [UnityTest] public IEnumerator EventsFailedWhenClear()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = TestUtils.IncorrectImageURL;
            var startTime = DateTime.Now;

            var future = new FutureSprite(url);
            var futureListener = new FutureListener<Sprite>(future);
            future.Timeout(TimeSpan.FromMilliseconds(100));

            LogAssert.ignoreFailingMessages = true;
            yield return future.StartLoading().ToCoroutine();
            LogAssert.ignoreFailingMessages = false;

            futureListener.Assert_Events_Equals(new List<EventName>
            {
                EventName.LoadingFromSource,
                EventName.Failed,
                EventName.Completed
            });
            futureListener.Assert_Events_Value(EventName.Completed, success => ((bool)success) == false);
            futureListener.Assert_Events_NotContains(EventName.Canceled);

            var task1 = future.AsTask();
            Assert.True(task1.IsCompleted);
            Assert.AreEqual(FutureStatus.FailedToLoad, future.Status);

            future.Cancel();
            Assert.AreEqual(FutureStatus.FailedToLoad, future.Status);

            var task2 = future.AsTask();
            Assert.True(task2.IsCompleted);
            Assert.AreEqual(FutureStatus.FailedToLoad, future.Status);

            future.Dispose();
        }
    }
}