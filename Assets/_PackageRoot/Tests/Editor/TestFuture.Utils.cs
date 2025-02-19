using NUnit.Framework;
using Cysharp.Threading.Tasks;
using System.Collections;
using Extensions.Unity.ImageLoader.Tests.Utils;

namespace Extensions.Unity.ImageLoader.Tests
{
    public partial class TestFuture
    {
        public static IEnumerable LoadFromMemoryCache(string url) => Load(url, null, FutureLoadedFrom.MemoryCache);
        public static IEnumerable Load(string url, FutureLoadingFrom? expectedLoadingFrom, FutureLoadedFrom expectedLoadedFrom)
        {
            var future1 = ImageLoader.LoadSprite(url);
            var futureListener = future1.ToFutureListener();

            if (expectedLoadingFrom.HasValue)
                futureListener.Assert_Events_Contains(expectedLoadingFrom.Value.ToEventName());

            var task1 = future1.AsTask();
            yield return future1.AsCoroutine();
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

            futureListener.Assert_Events_Value<bool>(EventName.Completed, value => value == false);

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
        public static IEnumerable LoadFromMemoryCacheThenCancel(string url) => LoadThenCancel(url, null, FutureLoadedFrom.MemoryCache);
        public static IEnumerable LoadThenCancel(string url, FutureLoadingFrom? expectedLoadingFrom, FutureLoadedFrom expectedLoadedFrom)
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

            futureListener.Assert_Events_Value<bool>(EventName.Completed, value => value == false);

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
        public static IEnumerable LoadAndCancel(string url, FutureLoadingFrom? expectedLoadingFrom)
        {
            var future1 = ImageLoader.LoadSprite(url);
            var futureListener = future1.ToFutureListener();

            if (expectedLoadingFrom.HasValue)
                futureListener.Assert_Events_Contains(expectedLoadingFrom.Value.ToEventName());

            var task1 = future1.AsTask();
            future1.Cancel();
            var task2 = future1.AsTask();

            futureListener.Assert_Events_Contains(EventName.Canceled);

            if (expectedLoadingFrom.HasValue)
                futureListener.Assert_Events_Equals(expectedLoadingFrom.Value.ToEventName(), EventName.Canceled, EventName.Completed);
            else
                futureListener.Assert_Events_Equals(EventName.Canceled, EventName.Completed);

            futureListener.Assert_Events_Value<bool>(EventName.Completed, value => value == false);

            Assert.IsTrue(task1.IsCompleted, "Task was not cancelled but Future was cancelled. Probably the OnCancel subscription was cleaned up too early.");
            Assert.IsTrue(task2.IsCompleted, "Task was not cancelled but Future was cancelled. Probably the OnCancel subscription was cleaned up too early.");

            yield return UniTask.Yield();

            if (expectedLoadingFrom.HasValue)
                futureListener.Assert_Events_Equals(expectedLoadingFrom.Value.ToEventName(), EventName.Canceled, EventName.Completed);
            else
                futureListener.Assert_Events_Equals(EventName.Canceled, EventName.Completed);

            futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == false);

            future1.ToFutureListener()
                .Assert_Events_Equals(EventName.Canceled, EventName.Completed)
                .Assert_Events_Value<bool>(EventName.Completed, success => success == false);

            future1.Dispose();
            yield return UniTask.Yield();
        }
    }
}