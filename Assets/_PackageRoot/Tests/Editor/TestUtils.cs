using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace Extensions.Unity.ImageLoader.Tests
{
    internal static class TestUtils
    {
        public static IEnumerator ClearEverything()
        {
            Debug.Log("ClearEverything ---------------------------- ");
            LogAssert.ignoreFailingMessages = true;
            UniTaskScheduler.UnobservedExceptionWriteLogType = LogType.Exception;
            ImageLoader.ClearRef();
            yield return ImageLoader.ClearCache().AsUniTask().ToCoroutine();
            LogAssert.ignoreFailingMessages = false;
        }
    }
}