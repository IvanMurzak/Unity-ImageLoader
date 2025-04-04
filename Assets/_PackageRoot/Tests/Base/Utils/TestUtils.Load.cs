using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    public static partial class TestUtils
    {
        public static IEnumerator LoadFromMemoryCache(string url, bool usePlaceholder = false) => Load(url, null, FutureLoadedFrom.MemoryCache, usePlaceholder);
        public static IEnumerator Load(string url, FutureLoadingFrom? expectedLoadingFrom, FutureLoadedFrom expectedLoadedFrom, bool usePlaceholder = false)
        {
            var future = ImageLoader.LoadSprite(url);
            var futureListener = future.ToFutureListener(ignorePlaceholder: !usePlaceholder);

            if (usePlaceholder)
            {
                future.SetPlaceholder(placeholderSprites[PlaceholderTrigger.LoadingFromDiskCache], PlaceholderTrigger.LoadingFromDiskCache);
                future.SetPlaceholder(placeholderSprites[PlaceholderTrigger.LoadingFromSource], PlaceholderTrigger.LoadingFromSource);
                future.SetPlaceholder(placeholderSprites[PlaceholderTrigger.FailedToLoad], PlaceholderTrigger.FailedToLoad);
                future.SetPlaceholder(placeholderSprites[PlaceholderTrigger.Canceled], PlaceholderTrigger.Canceled);
            }

            if (expectedLoadingFrom.HasValue)
                futureListener.Assert_Events_Contains(expectedLoadingFrom.Value.ToEventName());

            var task1 = future.AsTask();
            yield return future.TimeoutCoroutine(TimeSpan.FromSeconds(10));
            var task2 = future.AsTask();

            futureListener.Assert_Events_NotContains(EventName.Canceled);

            var events = expectedLoadingFrom.HasValue
                ? usePlaceholder
                    ? new [] { expectedLoadingFrom.Value.ToEventName(), EventName.Consumed, expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Consumed, EventName.Completed }
                    : new [] { expectedLoadingFrom.Value.ToEventName(), expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Completed }
                : usePlaceholder
                    ? new [] { expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Consumed, EventName.Completed }
                    : new [] { expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Completed };

            futureListener.Assert_Events_Equals(events);
            futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            Assert.IsTrue(task1.IsCompleted, "Task was not cancelled but Future was cancelled");
            Assert.IsTrue(task2.IsCompleted, "Task was not cancelled but Future was cancelled");

            yield return UniTask.Yield();

            futureListener.Assert_Events_Equals(events);
            futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            Assert.AreEqual(future.Status, expectedLoadedFrom.AsFutureStatus());

            future.ToFutureListener(ignoreLoadingWhenLoaded: true, ignorePlaceholder: true)
                .Assert_Events_Equals(expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Completed)
                .Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            future.ToFutureListener(ignoreLoadingWhenLoaded: true, ignorePlaceholder: false)
                .Assert_Events_Equals(expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Consumed, EventName.Completed)
                .Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            if (expectedLoadingFrom.HasValue)
                future.ToFutureListener()
                    .Assert_Events_Equals(expectedLoadingFrom.Value.ToEventName(), expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Completed)
                    .Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            future.Dispose();
            yield return UniTask.Yield();
        }
        public static IEnumerator LoadFromMemoryCacheThenCancel(string url, bool useGC, bool usePlaceholder = false)
            => LoadThenCancel(url, null, FutureLoadedFrom.MemoryCache, useGC, usePlaceholder);
        public static IEnumerator LoadThenCancel(string url, FutureLoadingFrom? expectedLoadingFrom, FutureLoadedFrom expectedLoadedFrom, bool useGC, bool usePlaceholder = false)
        {
            var future = ImageLoader.LoadSprite(url);
            var futureListener = future.ToFutureListener(ignorePlaceholder: !usePlaceholder);

            if (usePlaceholder)
            {
                future.SetPlaceholder(placeholderSprites[PlaceholderTrigger.LoadingFromDiskCache], PlaceholderTrigger.LoadingFromDiskCache);
                future.SetPlaceholder(placeholderSprites[PlaceholderTrigger.LoadingFromSource], PlaceholderTrigger.LoadingFromSource);
                future.SetPlaceholder(placeholderSprites[PlaceholderTrigger.FailedToLoad], PlaceholderTrigger.FailedToLoad);
                future.SetPlaceholder(placeholderSprites[PlaceholderTrigger.Canceled], PlaceholderTrigger.Canceled);
            }

            if (expectedLoadingFrom.HasValue)
                futureListener.Assert_Events_Contains(expectedLoadingFrom.Value.ToEventName());

            var task1 = future.AsTask();
            yield return future.TimeoutCoroutine(TimeSpan.FromSeconds(10));
            var task2 = future.AsTask();

            futureListener.Assert_Events_NotContains(EventName.Canceled);

            if (useGC)
                TestUtils.WaitForGCFast();

            future.Cancel();

            var events = expectedLoadingFrom.HasValue
                ? usePlaceholder
                    ? new [] { expectedLoadingFrom.Value.ToEventName(), EventName.Consumed, expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Consumed, EventName.Completed }
                    : new [] { expectedLoadingFrom.Value.ToEventName(), expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Completed }
                : usePlaceholder
                    ? new [] { expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Consumed, EventName.Completed }
                    : new [] { expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Completed };

            futureListener.Assert_Events_Equals(events);
            futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            Assert.IsTrue(task1.IsCompleted, "Task was not cancelled but Future was cancelled. Probably the OnCancel subscription was cleaned up too early.");
            Assert.IsTrue(task2.IsCompleted, "Task was not cancelled but Future was cancelled. Probably the OnCancel subscription was cleaned up too early.");

            yield return UniTask.Yield();

            futureListener.Assert_Events_Equals(events);
            futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            Assert.AreEqual(future.Status, expectedLoadedFrom.AsFutureStatus());

            future.ToFutureListener(ignoreLoadingWhenLoaded: true, ignorePlaceholder: true)
                .Assert_Events_Equals(expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Completed)
                .Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            future.ToFutureListener(ignoreLoadingWhenLoaded: true, ignorePlaceholder: false)
                .Assert_Events_Equals(expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Consumed, EventName.Completed)
                .Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            if (expectedLoadingFrom.HasValue)
                future.ToFutureListener()
                    .Assert_Events_Equals(expectedLoadingFrom.Value.ToEventName(), expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Completed)
                    .Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            future.Dispose();
            yield return UniTask.Yield();
        }
        public static IEnumerator LoadFromMemoryCacheAndCancel(string url, bool usePlaceholder = false) => LoadAndCancel(url, null, usePlaceholder);
        public static IEnumerator LoadAndCancel(string url, FutureLoadingFrom? expectedLoadingFrom, bool usePlaceholder = false)
        {
            yield return LoadAndCancel(url, expectedLoadingFrom, useGC: true, usePlaceholder);
            yield return LoadAndCancel(url, expectedLoadingFrom, useGC: false, usePlaceholder);
        }
        public static IEnumerator LoadAndCancel(string url, FutureLoadingFrom? expectedLoadingFrom, bool useGC, bool usePlaceholder = false)
        {
            var future = ImageLoader.LoadSprite(url);
            var futureListener = future.ToFutureListener(ignorePlaceholder: !usePlaceholder);
            var shouldLoadFromMemoryCache = !expectedLoadingFrom.HasValue;

            if (usePlaceholder)
            {
                future.SetPlaceholder(placeholderSprites[PlaceholderTrigger.LoadingFromDiskCache], PlaceholderTrigger.LoadingFromDiskCache);
                future.SetPlaceholder(placeholderSprites[PlaceholderTrigger.LoadingFromSource], PlaceholderTrigger.LoadingFromSource);
                future.SetPlaceholder(placeholderSprites[PlaceholderTrigger.FailedToLoad], PlaceholderTrigger.FailedToLoad);
                future.SetPlaceholder(placeholderSprites[PlaceholderTrigger.Canceled], PlaceholderTrigger.Canceled);
            }

            futureListener.Assert_Events_Contains(expectedLoadingFrom.HasValue
                ? expectedLoadingFrom.Value.ToEventName()
                : EventName.LoadedFromMemoryCache);

            if (useGC)
                TestUtils.WaitForGCFast();

            var task1 = future.AsTask();
            future.Cancel();
            var task2 = future.AsTask();

            var events = shouldLoadFromMemoryCache
                ? usePlaceholder
                    ? new [] { EventName.LoadedFromMemoryCache, EventName.Loaded, EventName.Consumed, EventName.Completed }
                    : new [] { EventName.LoadedFromMemoryCache, EventName.Loaded, EventName.Completed }
                : usePlaceholder
                    ? new [] { expectedLoadingFrom.Value.ToEventName(), EventName.Consumed, EventName.Canceled, EventName.Consumed, EventName.Completed }
                    : new [] { expectedLoadingFrom.Value.ToEventName(), EventName.Canceled, EventName.Completed };

            futureListener.Assert_Events_Equals(events);
            futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == shouldLoadFromMemoryCache);

            Assert.IsTrue(task1.IsCompleted, "Task was not cancelled but Future was cancelled. Probably the OnCancel subscription was cleaned up too early.");
            Assert.IsTrue(task2.IsCompleted, "Task was not cancelled but Future was cancelled. Probably the OnCancel subscription was cleaned up too early.");

            yield return UniTask.Yield();

            futureListener.Assert_Events_Equals(events);
            futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == shouldLoadFromMemoryCache);

            Assert.AreEqual(future.Status, shouldLoadFromMemoryCache
                ? FutureStatus.LoadedFromMemoryCache
                : FutureStatus.Canceled);

            var lateEvents = shouldLoadFromMemoryCache
                ? usePlaceholder
                    ? new[] { EventName.LoadedFromMemoryCache, EventName.Loaded, EventName.Consumed, EventName.Completed }
                    : new[] { EventName.LoadedFromMemoryCache, EventName.Loaded, EventName.Completed }
                : usePlaceholder
                    ? new[] { expectedLoadingFrom.Value.ToEventName(), EventName.Canceled, EventName.Consumed, EventName.Completed }
                    : new[] { expectedLoadingFrom.Value.ToEventName(), EventName.Canceled, EventName.Completed };

            var lateFutureListener = future.ToFutureListener(ignorePlaceholder: !usePlaceholder)
                .Assert_Events_Equals(lateEvents)
                .Assert_Events_Value<bool>(EventName.Completed, success => success == shouldLoadFromMemoryCache);

            if (expectedLoadingFrom.HasValue && future.IsLoaded)
                future.ToFutureListener(ignoreLoadingWhenLoaded: true, ignorePlaceholder: !usePlaceholder)
                    .Assert_Events_Equals(lateEvents.Except(new [] { expectedLoadingFrom.Value.ToEventName() }))
                    .Assert_Events_Value<bool>(EventName.Completed, success => success == shouldLoadFromMemoryCache);

            future.Dispose();
            yield return UniTask.Yield();
        }
    }
}