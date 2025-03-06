using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    public static partial class TestUtils
    {
        // public static IEnumerator PlaceholderFromMemoryCache(string url) => Placeholder(url, null, FutureLoadedFrom.MemoryCache);
        public static IEnumerator Placeholder(string url, params FuturePlaceholderTrigger[] triggers)
        {
            var future = ImageLoader.LoadSprite(url);
            var futureListener = future.ToFutureListener();

            var spritePlaceholder = Texture2D.whiteTexture.ToSprite();

            future.SetPlaceholder(spritePlaceholder, triggers);


            // var loadedFromMemoryCache = triggers.Contains(FuturePlaceholderTrigger.LoadedFromMemoryCache);
            // if (triggers.Any.HasValue)
            //     futureListener.Assert_Events_Contains(expectedLoadingFrom.Value.ToEventName());

            // var task1 = future.AsTask();
            // yield return future.TimeoutCoroutine(TimeSpan.FromSeconds(10));
            // var task2 = future.AsTask();

            // futureListener.Assert_Events_NotContains(EventName.Canceled);

            // if (expectedLoadingFrom.HasValue)
            //     futureListener.Assert_Events_Equals(expectedLoadingFrom.Value.ToEventName(), expectedLoadedFrom.ToEventName(), EventName.Then, EventName.Completed);
            // else
            //     futureListener.Assert_Events_Equals(expectedLoadedFrom.ToEventName(), EventName.Then, EventName.Completed);

            // futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            // Assert.IsTrue(task1.IsCompleted, "Task was not cancelled but Future was cancelled");
            // Assert.IsTrue(task2.IsCompleted, "Task was not cancelled but Future was cancelled");

            // yield return UniTask.Yield();

            // if (expectedLoadingFrom.HasValue)
            //     futureListener.Assert_Events_Equals(expectedLoadingFrom.Value.ToEventName(), expectedLoadedFrom.ToEventName(), EventName.Then, EventName.Completed);
            // else
            //     futureListener.Assert_Events_Equals(expectedLoadedFrom.ToEventName(), EventName.Then, EventName.Completed);

            // futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            // future.ToFutureListener(ignoreLoadingWhenLoaded: true)
            //     .Assert_Events_Equals(expectedLoadedFrom.ToEventName(), EventName.Then, EventName.Completed)
            //     .Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            // if (expectedLoadingFrom.HasValue)
            //     future.ToFutureListener()
            //         .Assert_Events_Equals(expectedLoadingFrom.Value.ToEventName(), expectedLoadedFrom.ToEventName(), EventName.Then, EventName.Completed)
            //         .Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            // future.Dispose();
            yield return UniTask.Yield();
        }
    }
}