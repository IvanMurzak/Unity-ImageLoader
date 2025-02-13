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

        [UnityTest] public IEnumerator Load5ReferencesInParallelLateDisposeNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadXReferencesInParallelLateDispose(5);
        }
        [UnityTest] public IEnumerator Load5ReferencesInParallelLateDispose()
        {
            yield return LoadXReferencesInParallelLateDispose(5);
        }
        [UnityTest] public IEnumerator Load1000ReferencesInParallelLateDisposeNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadXReferencesInParallelLateDispose(1000);
        }
        [UnityTest] public IEnumerator Load1000ReferencesInParallelLateDispose()
        {
            yield return LoadXReferencesInParallelLateDispose(1000);
        }
        public IEnumerator LoadXReferencesInParallelLateDispose(int count)
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = ImageURLs[0];

            var tasks = Enumerable.Range(0, count)
                .Select(i => Task.Run(() =>
                {
                    var futureRef = ImageLoader.LoadSpriteRef(url1);
                    Assert.NotNull(futureRef);
                    return futureRef.GetAwaiter().GetResult();
                }))
                .ToArray();

            yield return Task.WhenAll(tasks).AsUniTask().ToCoroutine();
            Assert.AreEqual(count, Reference<Sprite>.Counter(url1));

            foreach (var reference in tasks.Select(task => task.Result))
                reference.Dispose();
            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));
        }

        [UnityTest] public IEnumerator LoadOneMake5ReferencesInParallelLateDisposeNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadOneMake5ReferencesInParallelLateDispose();
        }
        [UnityTest] public IEnumerator LoadOneMake5ReferencesInParallelLateDispose()
        {
            yield return LoadOneMakeXReferencesInParallelLateDispose(5);
        }
        [UnityTest] public IEnumerator LoadOneMake1000ReferencesInParallelLateDisposeNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadOneMake1000ReferencesInParallelLateDispose();
        }
        [UnityTest] public IEnumerator LoadOneMake1000ReferencesInParallelLateDispose()
        {
            yield return LoadOneMakeXReferencesInParallelLateDispose(1000);
        }
        public IEnumerator LoadOneMakeXReferencesInParallelLateDispose(int count)
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