using NUnit.Framework;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine;
using System;
using Moq;
using Extensions.Unity.ImageLoader.Tests.Utils;
using System.Threading;

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
            var completed = false;
            var cancelled = false;
            var startTime = DateTime.Now;
            var consumer = new FakeConsumer<Sprite>();

            // Setup
            var mockFuture = new Mock<IFuture<Sprite>>();
            var sequence = new MockSequence();
            mockFuture.InSequence(sequence).Setup(f => f.LoadingFromSource(It.IsAny<Action>())).Callback<Action>(callback => callback());
            mockFuture.InSequence(sequence).Setup(f => f.LoadedFromSource(It.IsAny<Action<Sprite>>())).Callback<Action<Sprite>>(callback => callback(null));
            mockFuture.InSequence(sequence).Setup(f => f.Then(It.IsAny<Action<Sprite>>())).Callback<Action<Sprite>>(callback => callback(null));
            mockFuture.InSequence(sequence).Setup(f => f.Completed(It.IsAny<Action<bool>>())).Callback<Action<bool>>(callback => callback(true));

            var mockObject = mockFuture.Object;

            // Act
            var future = new FutureSprite(url)
                .PassEvents(mockObject, passCancelled: true)
                .LoadingFromSource(() => Debug.Log("LoadingFromSource"))
                .LoadedFromSource(x => Debug.Log("LoadedFromSource"))
                .Then(x => Debug.Log("Then"))
                .ThenSet(FakeConsumer<Sprite>.Setter, consumer)
                .Completed(success => completed = true);

            var task = future.StartLoading().AsTask();
            yield return new WaitUntil(() => task.IsCompleted);

            // Assert
            mockFuture.Verify(f => f.LoadingFromSource(It.IsAny<Action>()), Times.Once);
            mockFuture.Verify(f => f.LoadedFromSource(consumer.Consume), Times.Once);
            mockFuture.Verify(f => f.Then(It.IsAny<Action<Sprite>>()), Times.Once);
            mockFuture.Verify(f => f.Completed(It.IsAny<Action<bool>>()), Times.Once);


            Assert.IsFalse(completed);
            Assert.IsFalse(cancelled);
            var task1 = future.AsTask();
            future.Cancel();
            var task2 = future.AsTask();

            Assert.IsFalse(completed);
            Assert.IsTrue(cancelled);

            while (!task1.IsCompleted)
            {
                Assert.Less(DateTime.Now - startTime, TimeSpan.FromSeconds(2));
                yield return null;
            }
            while (!task2.IsCompleted)
            {
                Assert.Less(DateTime.Now - startTime, TimeSpan.FromSeconds(2));
                yield return null;
            }

            Assert.IsFalse(completed);
            Assert.IsTrue(cancelled);
            yield return UniTask.Delay(TimeSpan.FromSeconds(1)).ToCoroutine();
            future.Dispose();
        }
    }
}