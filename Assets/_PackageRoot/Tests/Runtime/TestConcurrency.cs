using System;
using System.Linq;
using System.Collections;
using NUnit.Framework;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using UnityEngine;
using System.Threading.Tasks;
using Extensions.Unity.ImageLoader.Tests.Utils;

namespace Extensions.Unity.ImageLoader.Tests
{
    public class TestConcurrency : Test
    {
        [UnitySetUp] public override IEnumerator SetUp() => base.SetUp();
        [UnityTearDown] public override IEnumerator TearDown() => base.TearDown();

        [UnityTest] public IEnumerator Load____1_ReferencesInParallelLateDispose_NoLogs() => TestUtils.RunNoLogs(Load____1_ReferencesInParallelLateDispose);
        [UnityTest] public IEnumerator Load____1_ReferencesInParallelLateDispose()
        {
            yield return Load_X_ReferencesInParallelLateDispose(1);
        }
        [UnityTest] public IEnumerator Load____2_ReferencesInParallelLateDispose_NoLogs() => TestUtils.RunNoLogs(Load____2_ReferencesInParallelLateDispose);
        [UnityTest] public IEnumerator Load____2_ReferencesInParallelLateDispose()
        {
            yield return Load_X_ReferencesInParallelLateDispose(2);
        }
        [UnityTest] public IEnumerator Load____5_ReferencesInParallelLateDispose_NoLogs() => TestUtils.RunNoLogs(Load____5_ReferencesInParallelLateDispose);
        [UnityTest] public IEnumerator Load____5_ReferencesInParallelLateDispose()
        {
            yield return Load_X_ReferencesInParallelLateDispose(5);
        }
        [UnityTest] public IEnumerator Load_1000_ReferencesInParallelLateDispose_NoLogs() => TestUtils.RunNoLogs(Load_1000_ReferencesInParallelLateDispose);
        [UnityTest] public IEnumerator Load_1000_ReferencesInParallelLateDispose()
        {
            yield return Load_X_ReferencesInParallelLateDispose(1000);
        }
        public IEnumerator Load_X_ReferencesInParallelLateDispose(int count, bool futureDispose = false)
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = TestUtils.ImageURLs[0];

            var cts = new System.Threading.CancellationTokenSource();
            cts.CancelAfterSlim(TimeSpan.FromSeconds(25) + TimeSpan.FromMilliseconds(5 * count));

            var tasks = Enumerable.Range(0, count)
                .Select(i => Task.Run(async () =>
                {
                    var futureRef = ImageLoader.LoadSpriteRef(url);
                    Assert.NotNull(futureRef);
                    var result = await futureRef;
                    if (futureDispose)
                        futureRef.Dispose();
                    return result;
                }))
                .ToArray();

            var waitTask = Task.WhenAll(tasks);

            while (!waitTask.IsCompleted && !cts.Token.IsCancellationRequested)
                yield return UniTask.Yield();

            Assert.False(cts.Token.IsCancellationRequested, "Timeout");
            Assert.AreEqual(count, Reference<Sprite>.Counter(url));

            foreach (var reference in tasks.Select(task => task.Result))
                reference.Dispose();
            Assert.AreEqual(0, Reference<Sprite>.Counter(url));
        }

        [UnityTest] public IEnumerator LoadOneMake____5_ReferencesInParallelLateDispose_NoLogs() => TestUtils.RunNoLogs(LoadOneMake____5_ReferencesInParallelLateDispose);
        [UnityTest] public IEnumerator LoadOneMake____5_ReferencesInParallelLateDispose()
        {
            yield return LoadOneMake_X_ReferencesInParallelLateDispose(5);
        }
        [UnityTest] public IEnumerator LoadOneMake_1000_ReferencesInParallelLateDispose_NoLogs() => TestUtils.RunNoLogs(LoadOneMake_1000_ReferencesInParallelLateDispose);
        [UnityTest] public IEnumerator LoadOneMake_1000_ReferencesInParallelLateDispose()
        {
            yield return LoadOneMake_X_ReferencesInParallelLateDispose(1000);
        }
        public IEnumerator LoadOneMake_X_ReferencesInParallelLateDispose(int count)
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url = TestUtils.ImageURLs[0];

            var future = ImageLoader.LoadSpriteRef(url);
            var task1 = future.AsTask();

            yield return task1.TimeoutCoroutine(TimeSpan.FromSeconds(25));

            Assert.AreEqual(1, Reference<Sprite>.Counter(url));

            var ref1 = task1.Result;
            Assert.NotNull(ref1);

            var references = new Reference<Sprite>[count];
            var tasks = Enumerable.Range(0, count)
                .Select(i => Task.Run(() =>
                {
                    var ref0 = ImageLoader.LoadSpriteRefFromMemoryCache(url);
                    Assert.NotNull(ref0);
                    return ref0;
                }))
                .ToArray();

            yield return Task.WhenAll(tasks).TimeoutCoroutine(TimeSpan.FromSeconds(10));
            Assert.AreEqual(count + 1, Reference<Sprite>.Counter(url));

            foreach (var reference in tasks.Select(task => task.Result))
                reference.Dispose();
            Assert.AreEqual(1, Reference<Sprite>.Counter(url));

            ref1.Dispose();
            Assert.AreEqual(0, Reference<Sprite>.Counter(url));
        }
    }
}