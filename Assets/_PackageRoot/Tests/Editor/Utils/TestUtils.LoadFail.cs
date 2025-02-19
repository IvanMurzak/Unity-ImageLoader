using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    internal static partial class TestUtils
    {
        public static IEnumerator LoadFailFromMemoryCache(string url) => LoadFail(url, null);
        public static IEnumerator LoadFail(string url, FutureLoadingFrom? expectedLoadingFrom)
        {
            var timeout = TimeSpan.FromMilliseconds(100);
            var future1 = ImageLoader.LoadSprite(url).Timeout(timeout);
            var futureListener = future1.ToFutureListener();

            if (expectedLoadingFrom.HasValue)
                futureListener.Assert_Events_Contains(expectedLoadingFrom.Value.ToEventName());

            var task1 = future1.AsTask();
            if (expectedLoadingFrom.HasValue && expectedLoadingFrom.Value == FutureLoadingFrom.Source) // exception should be thrown only if ONLY loading from Source
                LogAssert.Expect(LogType.Error, $"[ImageLoader] Future[id={future1.Id}] Timeout ({timeout}): {url}");
            yield return UniTask.WaitWhile(() => future1.IsInProgress)
                .Timeout(TimeSpan.FromSeconds(10))
                .ToCoroutine();
            var task2 = future1.AsTask();

            var events = expectedLoadingFrom.HasValue
                ? new[] { expectedLoadingFrom.Value.ToEventName(), EventName.Failed, EventName.Completed }
                : new[] { EventName.Failed, EventName.Completed};

            futureListener.Assert_Events_Equals(events);
            futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == false);

            Assert.IsTrue(task1.IsCompleted, "Task was not cancelled but Future was cancelled");
            Assert.IsTrue(task2.IsCompleted, "Task was not cancelled but Future was cancelled");

            yield return UniTask.Yield();

            futureListener.Assert_Events_Equals(events);
            futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == false);

            future1.ToFutureListener(ignoreLoadingWhenLoaded: true)
                .Assert_Events_Equals(events)
                .Assert_Events_Value<bool>(EventName.Completed, success => success == false);

            if (expectedLoadingFrom.HasValue)
                future1.ToFutureListener()
                    .Assert_Events_Equals(events)
                    .Assert_Events_Value<bool>(EventName.Completed, success => success == false);

            future1.Dispose();
            yield return UniTask.Yield();
        }
    }
}