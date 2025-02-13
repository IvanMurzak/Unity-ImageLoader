using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace Extensions.Unity.ImageLoader.Tests
{
    internal static class TestUtils
    {
        public static IEnumerator ClearEverything(string message)
        {
            Debug.Log(message.PadRight(50, '-'));
            LogAssert.ignoreFailingMessages = true;
            UniTaskScheduler.UnobservedExceptionWriteLogType = LogType.Exception;
            ImageLoader.ClearRef();
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();

            GC.Collect(100, GCCollectionMode.Forced, blocking: true);
            GC.WaitForPendingFinalizers();
            LogAssert.ignoreFailingMessages = false;
        }
    }
}