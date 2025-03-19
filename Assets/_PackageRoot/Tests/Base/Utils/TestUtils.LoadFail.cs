using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    public static partial class TestUtils
    {
        public static IEnumerator LoadFailFromMemoryCache(string url, bool usePlaceholder = false) => LoadFail(url, null, usePlaceholder);
        public static IEnumerator LoadFail(string url, FutureLoadingFrom? expectedLoadingFrom, bool usePlaceholder = false)
        {
            var timeout = TimeSpan.FromMilliseconds(100);
            var future = ImageLoader.LoadSprite(url).Timeout(timeout);
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

// TODO: To find a way to use it in a player build
#if UNITY_EDITOR
            if (expectedLoadingFrom.HasValue && expectedLoadingFrom.Value == FutureLoadingFrom.Source) // exception should be thrown only if ONLY loading from Source
                UnityEngine.TestTools.LogAssert.Expect(LogType.Error, $"[ImageLoader] Future[id={future.Id}] Timeout ({timeout}): {url}"); // compilation error in player build
#endif
            yield return TestUtils.WaitWhile(() => future.IsInProgress, TimeSpan.FromSeconds(10));
            var task2 = future.AsTask();

            var events = expectedLoadingFrom.HasValue
                ? usePlaceholder
                    ? new[] { expectedLoadingFrom.Value.ToEventName(), EventName.Consumed, EventName.Failed, EventName.Consumed, EventName.Completed }
                    : new[] { expectedLoadingFrom.Value.ToEventName(), EventName.Failed, EventName.Completed }
                : usePlaceholder
                    ? new[] { EventName.Failed, EventName.Consumed, EventName.Completed}
                    : new[] { EventName.Failed, EventName.Completed};

            futureListener.Assert_Events_Equals(events);
            futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == false);

            Assert.IsTrue(task1.IsCompleted, "Task was not cancelled but Future was cancelled");
            Assert.IsTrue(task2.IsCompleted, "Task was not cancelled but Future was cancelled");

            yield return UniTask.Yield();

            futureListener.Assert_Events_Equals(events);
            futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == false);

            Assert.AreEqual(future.Status, FutureStatus.FailedToLoad);

            var lateEvents = expectedLoadingFrom.HasValue
                ? usePlaceholder
                    ? new[] { expectedLoadingFrom.Value.ToEventName(), EventName.Failed, EventName.Consumed, EventName.Completed }
                    : new[] { expectedLoadingFrom.Value.ToEventName(), EventName.Failed, EventName.Completed }
                : usePlaceholder
                    ? new[] { EventName.Failed, EventName.Consumed, EventName.Completed}
                    : new[] { EventName.Failed, EventName.Completed};

            future.ToFutureListener(ignoreLoadingWhenLoaded: true, ignorePlaceholder: !usePlaceholder)
                .Assert_Events_Equals(lateEvents)
                .Assert_Events_Value<bool>(EventName.Completed, success => success == false);

            if (expectedLoadingFrom.HasValue)
                future.ToFutureListener(ignorePlaceholder: !usePlaceholder)
                    .Assert_Events_Equals(lateEvents)
                    .Assert_Events_Value<bool>(EventName.Completed, success => success == false);

            future.Dispose();
            yield return UniTask.Yield();
        }
    }
}