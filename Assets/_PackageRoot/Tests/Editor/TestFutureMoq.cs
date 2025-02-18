using NUnit.Framework;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine;
using System;
using Extensions.Unity.ImageLoader.Tests.Utils;
using System.Threading;
using NSubstitute;
using System.Collections.Generic;

namespace Extensions.Unity.ImageLoader.Tests
{
    public class TestFutureMoq
    {
        static readonly string[] ImageURLs =
        {
            "https://github.com/IvanMurzak/Unity-ImageLoader/raw/master/Test%20Images/ImageA.jpg",
            "https://github.com/IvanMurzak/Unity-ImageLoader/raw/master/Test%20Images/ImageB.png",
            "https://github.com/IvanMurzak/Unity-ImageLoader/raw/master/Test%20Images/ImageC.png"
        };
        static string IncorrectImageURL => $"https://doesntexist.com/{Guid.NewGuid()}.png";

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
        [UnityTest] public IEnumerator EventsOrderWhenClearNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return EventsOrderWhenClear();
        }
        [UnityTest] public IEnumerator EventsOrderWhenClear()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = ImageURLs[0];
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
    }
}