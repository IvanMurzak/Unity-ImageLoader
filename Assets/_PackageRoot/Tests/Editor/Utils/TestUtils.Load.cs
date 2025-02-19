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
            var future1 = ImageLoader.LoadSprite(url);
            var futureListener = future1.ToFutureListener();

            if (expectedLoadingFrom.HasValue)
                futureListener.Assert_Events_Contains(expectedLoadingFrom.Value.ToEventName());

            var task1 = future1.AsTask();
            yield return future1.AsUniTask()
                .Timeout(TimeSpan.FromSeconds(10))
                .ToCoroutine();
            var task2 = future1.AsTask();

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

            future1.ToFutureListener(ignoreLoadingWhenLoaded: true)
                .Assert_Events_Equals(expectedLoadedFrom.ToEventName(), EventName.Then, EventName.Completed)
                .Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            if (expectedLoadingFrom.HasValue)
                future1.ToFutureListener()
                    .Assert_Events_Equals(expectedLoadingFrom.Value.ToEventName(), expectedLoadedFrom.ToEventName(), EventName.Then, EventName.Completed)
                    .Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            future1.Dispose();
            yield return UniTask.Yield();
        }
        public static IEnumerator LoadFromMemoryCacheThenCancel(string url) => LoadThenCancel(url, null, FutureLoadedFrom.MemoryCache);
        public static IEnumerator LoadThenCancel(string url, FutureLoadingFrom? expectedLoadingFrom, FutureLoadedFrom expectedLoadedFrom)
        {
            var future1 = ImageLoader.LoadSprite(url);
            var futureListener = future1.ToFutureListener();

            if (expectedLoadingFrom.HasValue)
                futureListener.Assert_Events_Contains(expectedLoadingFrom.Value.ToEventName());

            var task1 = future1.AsTask();
            yield return future1.AsCoroutine();
            var task2 = future1.AsTask();

            futureListener.Assert_Events_NotContains(EventName.Canceled);

            future1.Cancel();

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

            future1.ToFutureListener(ignoreLoadingWhenLoaded: true)
                .Assert_Events_Equals(expectedLoadedFrom.ToEventName(), EventName.Then, EventName.Completed)
                .Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            if (expectedLoadingFrom.HasValue)
                future1.ToFutureListener()
                    .Assert_Events_Equals(expectedLoadingFrom.Value.ToEventName(), expectedLoadedFrom.ToEventName(), EventName.Then, EventName.Completed)
                    .Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            future1.Dispose();
            yield return UniTask.Yield();
        }
        public static IEnumerator LoadFromMemoryCacheAndCancel(string url) => LoadAndCancel(url, null);
        public static IEnumerator LoadAndCancel(string url, FutureLoadingFrom? expectedLoadingFrom)
        {
            var future1 = ImageLoader.LoadSprite(url);
            var futureListener = future1.ToFutureListener();
            var shouldLoadFromMemoryCache = !expectedLoadingFrom.HasValue;

            futureListener.Assert_Events_Contains(expectedLoadingFrom.HasValue
                ? expectedLoadingFrom.Value.ToEventName()
                : EventName.LoadedFromMemoryCache);

            var task1 = future1.AsTask();
            future1.Cancel();
            var task2 = future1.AsTask();

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

            future1.ToFutureListener()
                .Assert_Events_Equals(events)
                .Assert_Events_Value<bool>(EventName.Completed, success => success == shouldLoadFromMemoryCache);

            if (expectedLoadingFrom.HasValue && future1.IsLoaded)
                future1.ToFutureListener(ignoreLoadingWhenLoaded: true)
                    .Assert_Events_Equals(events.Except(new [] { expectedLoadingFrom.Value.ToEventName() }))
                    .Assert_Events_Value<bool>(EventName.Completed, success => success == shouldLoadFromMemoryCache);

            future1.Dispose();
            yield return UniTask.Yield();
        }
    }
}