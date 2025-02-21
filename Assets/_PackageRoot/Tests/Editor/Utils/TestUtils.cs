using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    internal static partial class TestUtils
    {
        public static readonly string[] ImageURLs =
        {
            "https://github.com/IvanMurzak/Unity-ImageLoader/raw/master/Test%20Images/ImageA.jpg",
            "https://github.com/IvanMurzak/Unity-ImageLoader/raw/master/Test%20Images/ImageB.png",
            "https://github.com/IvanMurzak/Unity-ImageLoader/raw/master/Test%20Images/ImageC.png"
        };
        public static string IncorrectImageURL => $"https://doesntexist.com/{Guid.NewGuid()}.png";
        public static IEnumerable<string> IncorrectImageURLs(int count = 3) => Enumerable.Range(0, count).Select(_ => IncorrectImageURL);
        public static readonly byte[] CorruptedTextureBytes = new byte[] { 0 };

        public static IEnumerator WaitTicks(int ticks = 1)
        {
            for (var i = 0; i < ticks; i++)
                yield return UniTask.Yield();
        }
        public static IEnumerator ClearEverything(string message)
        {
            if (message != null)
                Debug.Log(message.PadRight(50, '-'));
            LogAssert.ignoreFailingMessages = true;
            UniTaskScheduler.UnobservedExceptionWriteLogType = LogType.Exception;
            ImageLoader.ClearSpriteRef();
            ImageLoader.ClearTextureRef();
            yield return ImageLoader.ClearCacheAll().AsUniTask().ToCoroutine();

            WaitForGCFast();
            LogAssert.ignoreFailingMessages = false;
        }
        public static void WaitForGCFast()
        {
            GC.Collect(100, GCCollectionMode.Forced, blocking: true);
            GC.WaitForPendingFinalizers();
        }
        public static IEnumerator WaitForGC(int millisecondsDelay = 100)
        {
            yield return UniTask.Delay(TimeSpan.FromMilliseconds(millisecondsDelay)).ToCoroutine();
            WaitForGCFast();
            yield return UniTask.Delay(TimeSpan.FromMilliseconds(millisecondsDelay)).ToCoroutine();
        }
        public static IEnumerator RunNoLogs(Func<IEnumerator> test)
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return test();
        }
    }
}