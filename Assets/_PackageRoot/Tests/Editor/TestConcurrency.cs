using System;
using System.Linq;
using System.Collections;
using NUnit.Framework;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using UnityEngine;
using System.Threading.Tasks;

namespace Extensions.Unity.ImageLoader.Tests
{
    public class TestConcurrency
    {
        static readonly string[] ImageURLs =
        {
            "https://github.com/IvanMurzak/Unity-ImageLoader/raw/master/Test%20Images/ImageA.jpg",
            "https://github.com/IvanMurzak/Unity-ImageLoader/raw/master/Test%20Images/ImageB.png",
            "https://github.com/IvanMurzak/Unity-ImageLoader/raw/master/Test%20Images/ImageC.png"
        };

        [UnitySetUp] public IEnumerator SetUp()
        {
            yield return TestUtils.ClearEverything();
            ImageLoader.settings.debugLevel = DebugLevel.Log;
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            yield return TestUtils.ClearEverything();
        }

        [UnityTest] public IEnumerator Load____5_ReferencesInParallelLateDisposeNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return Load_X_ReferencesInParallelLateDispose(5);
        }
        [UnityTest] public IEnumerator Load____5_ReferencesInParallelLateDispose()
        {
            yield return Load_X_ReferencesInParallelLateDispose(5);
        }
        [UnityTest] public IEnumerator Load_1000_ReferencesInParallelLateDisposeNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return Load_X_ReferencesInParallelLateDispose(1000);
        }
        [UnityTest] public IEnumerator Load_1000_ReferencesInParallelLateDispose()
        {
            yield return Load_X_ReferencesInParallelLateDispose(1000);
        }
        public IEnumerator Load_X_ReferencesInParallelLateDispose(int count)
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = ImageURLs[0];

            var cts = new System.Threading.CancellationTokenSource();
            cts.CancelAfterSlim(TimeSpan.FromSeconds(5) + TimeSpan.FromMilliseconds(5 * count));

            var tasks = Enumerable.Range(0, count)
                .Select(i => Task.Run(async () =>
                {
                    var futureRef = ImageLoader.LoadSpriteRef(url1);
                    Assert.NotNull(futureRef);
                    return await futureRef;
                }))
                .ToArray();

            var waitTask = Task.WhenAll(tasks);

            while (!waitTask.IsCompleted && !cts.Token.IsCancellationRequested)
                yield return null;

            Assert.False(cts.Token.IsCancellationRequested, "Timeout");
            Assert.AreEqual(count, Reference<Sprite>.Counter(url1));

            foreach (var reference in tasks.Select(task => task.Result))
                reference.Dispose();
            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));
        }

        [UnityTest] public IEnumerator LoadOneMake____5_ReferencesInParallelLateDisposeNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadOneMake_X_ReferencesInParallelLateDispose(5);
        }
        [UnityTest] public IEnumerator LoadOneMake____5_ReferencesInParallelLateDispose()
        {
            yield return LoadOneMake_X_ReferencesInParallelLateDispose(5);
        }
        [UnityTest] public IEnumerator LoadOneMake_1000_ReferencesInParallelLateDisposeNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadOneMake_X_ReferencesInParallelLateDispose(100);
        }
        [UnityTest] public IEnumerator LoadOneMake_1000_ReferencesInParallelLateDispose()
        {
            yield return LoadOneMake_X_ReferencesInParallelLateDispose(1000);
        }
        public IEnumerator LoadOneMake_X_ReferencesInParallelLateDispose(int count)
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = ImageURLs[0];

            var future = ImageLoader.LoadSpriteRef(url1);
            var task1 = future.AsTask();
            while (!task1.IsCompleted)
                yield return null;

            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));

            var ref1 = task1.Result;
            Assert.NotNull(ref1);

            var references = new Reference<Sprite>[count];
            var tasks = Enumerable.Range(0, count)
                .Select(i => Task.Run(() =>
                {
                    var ref0 = ImageLoader.LoadFromMemoryCacheRef(url1);
                    Assert.NotNull(ref0);
                    return ref0;
                }))
                .ToArray();

            yield return Task.WhenAll(tasks).AsUniTask().ToCoroutine();
            Assert.AreEqual(count + 1, Reference<Sprite>.Counter(url1));

            foreach (var reference in tasks.Select(task => task.Result))
                reference.Dispose();
            Assert.AreEqual(1, Reference<Sprite>.Counter(url1));

            ref1.Dispose();
            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));
        }
    }
}