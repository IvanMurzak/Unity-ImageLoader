using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace Extensions.Unity.ImageLoader.Tests
{
    internal static class TestUtils
    {
        public static readonly string[] ImageURLs =
        {
            "https://github.com/IvanMurzak/Unity-ImageLoader/raw/master/Test%20Images/ImageA.jpg",
            "https://github.com/IvanMurzak/Unity-ImageLoader/raw/master/Test%20Images/ImageB.png",
            "https://github.com/IvanMurzak/Unity-ImageLoader/raw/master/Test%20Images/ImageC.png"
        };
        public static string IncorrectImageURL => $"https://doesntexist.com/{Guid.NewGuid()}.png";

        public static IEnumerator ClearEverything(string message)
        {
            Debug.Log(message.PadRight(50, '-'));
            LogAssert.ignoreFailingMessages = true;
            UniTaskScheduler.UnobservedExceptionWriteLogType = LogType.Exception;
            ImageLoader.ClearRef();
            yield return ImageLoader.ClearCacheAll().AsUniTask().ToCoroutine();

            GC.Collect(100, GCCollectionMode.Forced, blocking: true);
            GC.WaitForPendingFinalizers();
            LogAssert.ignoreFailingMessages = false;
        }
        public static IEnumerator WaitForGC(int millisecondsDelay = 100)
        {
            yield return UniTask.Delay(TimeSpan.FromMilliseconds(millisecondsDelay)).ToCoroutine();
            GC.Collect(100, GCCollectionMode.Forced, blocking: true);
            GC.WaitForPendingFinalizers();
            yield return UniTask.Delay(TimeSpan.FromMilliseconds(millisecondsDelay)).ToCoroutine();
        }
    }
}