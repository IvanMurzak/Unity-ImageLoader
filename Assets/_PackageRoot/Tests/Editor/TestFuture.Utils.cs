using NUnit.Framework;
using Cysharp.Threading.Tasks;
using System.Collections;
using Extensions.Unity.ImageLoader.Tests.Utils;

namespace Extensions.Unity.ImageLoader.Tests
{
    public partial class TestFuture
    {
        public static IEnumerable LoadAndCancel(string url, FutureLoadingFrom expectedLoadingFrom)
        {
            var future1 = ImageLoader.LoadSprite(url);
            var futureListener = future1.ToFutureListener();

            futureListener.Assert_Events_Contains(expectedLoadingFrom.ToEventName());

            var task1 = future1.AsTask();
            future1.Cancel();
            var task2 = future1.AsTask();

            futureListener.Assert_Events_Contains(EventName.Canceled);
            futureListener.Assert_Events_Equals(expectedLoadingFrom.ToEventName(), EventName.Canceled, EventName.Completed);
            futureListener.Assert_Events_Value<bool>(EventName.Completed, value => value == false);

            Assert.IsTrue(task1.IsCompleted, "Task was not cancelled but Future was cancelled. Probably the OnCancel subscription was cleaned up too early.");
            Assert.IsTrue(task2.IsCompleted, "Task was not cancelled but Future was cancelled. Probably the OnCancel subscription was cleaned up too early.");

            yield return UniTask.Yield();

            futureListener.Assert_Events_Equals(expectedLoadingFrom.ToEventName(), EventName.Canceled, EventName.Completed);
            futureListener.Assert_Events_Value<bool>(EventName.Completed, value => value == false);

            var futureListener2 = future1.ToFutureListener()
                .Assert_Events_Equals(EventName.Canceled, EventName.Completed)
                .Assert_Events_Value<bool>(EventName.Completed, value => value == false);

            future1.Dispose();
        }
    }
}