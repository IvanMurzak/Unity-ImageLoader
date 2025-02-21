using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    internal static partial class TestUtils
    {
        public static IEnumerator LoadFromMemoryCache(string url) => Load(url, null, FutureLoadedFrom.MemoryCache);
        public static IEnumerator Load(string url, FutureLoadingFrom? expectedLoadingFrom, FutureLoadedFrom expectedLoadedFrom)
        {
            var future = ImageLoader.LoadSprite(url);
            var futureListener = future.ToFutureListener();

            if (expectedLoadingFrom.HasValue)
                futureListener.Assert_Events_Contains(expectedLoadingFrom.Value.ToEventName());

            var task1 = future.AsTask();
            yield return future.AsUniTask()
                .Timeout(TimeSpan.FromSeconds(10))
                .ToCoroutine();
            var task2 = future.AsTask();

            futureListener.Assert_Events_NotContains(EventName.Canceled);

            if (expectedLoadingFrom.HasValue)
                futureListener.Assert_Events_Equals(expectedLoadingFrom.Value.ToEventName(), expectedLoadedFrom.ToEventName(), EventName.Then, EventName.Completed);
            else
                futureListener.Assert_Events_Equals(expectedLoadedFrom.ToEventName(), EventName.Then, EventName.Completed);

            futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            Assert.IsTrue(task1.IsCompleted, "Task was not cancelled but Future was cancelled");
            Assert.IsTrue(task2.IsCompleted, "Task was not cancelled but Future was cancelled");

            yield return UniTask.Yield();

            if (expectedLoadingFrom.HasValue)
                futureListener.Assert_Events_Equals(expectedLoadingFrom.Value.ToEventName(), expectedLoadedFrom.ToEventName(), EventName.Then, EventName.Completed);
            else
                futureListener.Assert_Events_Equals(expectedLoadedFrom.ToEventName(), EventName.Then, EventName.Completed);

            futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            future.ToFutureListener(ignoreLoadingWhenLoaded: true)
                .Assert_Events_Equals(expectedLoadedFrom.ToEventName(), EventName.Then, EventName.Completed)
                .Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            if (expectedLoadingFrom.HasValue)
                future.ToFutureListener()
                    .Assert_Events_Equals(expectedLoadingFrom.Value.ToEventName(), expectedLoadedFrom.ToEventName(), EventName.Then, EventName.Completed)
                    .Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            future.Dispose();
            yield return UniTask.Yield();
        }
        public static IEnumerator LoadFromMemoryCacheThenCancel(string url, bool useGC) => LoadThenCancel(url, null, FutureLoadedFrom.MemoryCache, useGC);

        // public static IEnumerator LoadThenCancel(string url, FutureLoadingFrom? expectedLoadingFrom, FutureLoadedFrom expectedLoadedFrom)
        // {
        //     yield return LoadThenCancel(url, expectedLoadingFrom, expectedLoadedFrom, useGC: true);
        //     yield return LoadThenCancel(url, expectedLoadingFrom, expectedLoadedFrom, useGC: false);
        // }
        public static IEnumerator LoadThenCancel(string url, FutureLoadingFrom? expectedLoadingFrom, FutureLoadedFrom expectedLoadedFrom, bool useGC)
        {
            var future = ImageLoader.LoadSprite(url);
            var futureListener = future.ToFutureListener();

            if (expectedLoadingFrom.HasValue)
                futureListener.Assert_Events_Contains(expectedLoadingFrom.Value.ToEventName());

            var task1 = future.AsTask();
            yield return future.AsCoroutine();
            var task2 = future.AsTask();

            futureListener.Assert_Events_NotContains(EventName.Canceled);

            if (useGC)
                TestUtils.WaitForGCFast();

            future.Cancel();

            if (expectedLoadingFrom.HasValue)
                futureListener.Assert_Events_Equals(expectedLoadingFrom.Value.ToEventName(), expectedLoadedFrom.ToEventName(), EventName.Then, EventName.Completed);
            else
                futureListener.Assert_Events_Equals(expectedLoadedFrom.ToEventName(), EventName.Then, EventName.Completed);

            futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            Assert.IsTrue(task1.IsCompleted, "Task was not cancelled but Future was cancelled. Probably the OnCancel subscription was cleaned up too early.");
            Assert.IsTrue(task2.IsCompleted, "Task was not cancelled but Future was cancelled. Probably the OnCancel subscription was cleaned up too early.");

            yield return UniTask.Yield();

            if (expectedLoadingFrom.HasValue)
                futureListener.Assert_Events_Equals(expectedLoadingFrom.Value.ToEventName(), expectedLoadedFrom.ToEventName(), EventName.Then, EventName.Completed);
            else
                futureListener.Assert_Events_Equals(expectedLoadedFrom.ToEventName(), EventName.Then, EventName.Completed);

            futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            future.ToFutureListener(ignoreLoadingWhenLoaded: true)
                .Assert_Events_Equals(expectedLoadedFrom.ToEventName(), EventName.Then, EventName.Completed)
                .Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            if (expectedLoadingFrom.HasValue)
                future.ToFutureListener()
                    .Assert_Events_Equals(expectedLoadingFrom.Value.ToEventName(), expectedLoadedFrom.ToEventName(), EventName.Then, EventName.Completed)
                    .Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            future.Dispose();
            yield return UniTask.Yield();
        }
        public static IEnumerator LoadFromMemoryCacheAndCancel(string url) => LoadAndCancel(url, null);
        public static IEnumerator LoadAndCancel(string url, FutureLoadingFrom? expectedLoadingFrom)
        {
            yield return LoadAndCancel(url, expectedLoadingFrom, useGC: true);
            yield return LoadAndCancel(url, expectedLoadingFrom, useGC: false);
        }
        public static IEnumerator LoadAndCancel(string url, FutureLoadingFrom? expectedLoadingFrom, bool useGC)
        {
            var future = ImageLoader.LoadSprite(url);
            var futureListener = future.ToFutureListener();
            var shouldLoadFromMemoryCache = !expectedLoadingFrom.HasValue;

            futureListener.Assert_Events_Contains(expectedLoadingFrom.HasValue
                ? expectedLoadingFrom.Value.ToEventName()
                : EventName.LoadedFromMemoryCache);

            if (useGC)
                TestUtils.WaitForGCFast();

            var task1 = future.AsTask();
            future.Cancel();
            var task2 = future.AsTask();

            var events = shouldLoadFromMemoryCache
                ? new [] { EventName.LoadedFromMemoryCache, EventName.Then, EventName.Completed }
                : new [] { expectedLoadingFrom.Value.ToEventName(), EventName.Canceled, EventName.Completed };

            futureListener.Assert_Events_Equals(events);
            futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == shouldLoadFromMemoryCache);

            Assert.IsTrue(task1.IsCompleted, "Task was not cancelled but Future was cancelled. Probably the OnCancel subscription was cleaned up too early.");
            Assert.IsTrue(task2.IsCompleted, "Task was not cancelled but Future was cancelled. Probably the OnCancel subscription was cleaned up too early.");

            yield return UniTask.Yield();

            futureListener.Assert_Events_Equals(events);
            futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == shouldLoadFromMemoryCache);

            future.ToFutureListener()
                .Assert_Events_Equals(events)
                .Assert_Events_Value<bool>(EventName.Completed, success => success == shouldLoadFromMemoryCache);

            if (expectedLoadingFrom.HasValue && future.IsLoaded)
                future.ToFutureListener(ignoreLoadingWhenLoaded: true)
                    .Assert_Events_Equals(events.Except(new [] { expectedLoadingFrom.Value.ToEventName() }))
                    .Assert_Events_Value<bool>(EventName.Completed, success => success == shouldLoadFromMemoryCache);

            future.Dispose();
            yield return UniTask.Yield();
        }
    }
}