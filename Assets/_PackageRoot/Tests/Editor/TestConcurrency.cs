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

        [SetUp] public void SetUp() => ImageLoader.settings.debugLevel = DebugLevel.Log;

        [UnityTest] public IEnumerator LoadOneMake1000ReferencesInParallelLateDisposeNoLogs()
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return LoadOneMake1000ReferencesInParallelLateDispose();
        }
        [UnityTest] public IEnumerator LoadOneMake1000ReferencesInParallelLateDispose()
        {
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = true;

            var url1 = ImageURLs[0];

            var count = 1000;
            var references = new Reference<Sprite>[count];
            var tasks = Enumerable.Range(0, count)
                .Select(i => Task.Run(() =>
                {
                    var reference = ImageLoader.LoadFromMemoryCacheRef(url1);
                    Assert.NotNull(reference);
                    references[i] = reference;
                    return reference;
                }))
                .ToArray();

            yield return Task.WhenAll(tasks).AsUniTask().ToCoroutine();
            Assert.AreEqual(count, Reference<Sprite>.Counter(url1));

            foreach (var reference in references)
                reference.Dispose();
            Assert.AreEqual(0, Reference<Sprite>.Counter(url1));
        }
    }
}