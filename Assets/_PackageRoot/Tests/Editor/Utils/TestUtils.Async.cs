using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    internal static partial class TestUtils
    {
        public static IEnumerator WaitTicks(int ticks = 1)
        {
            for (var i = 0; i < ticks; i++)
                yield return UniTask.Yield();
        }
        public static IEnumerator WaitWhile(Func<bool> condition, TimeSpan timeout)
        {
            using (var timeoutToken = new CancellationTokenSource())
            using (var disposable = timeoutToken.CancelAfterSlim(timeout))
            yield return UniTask.WaitWhile(condition, cancellationToken: timeoutToken.Token).ToCoroutine();
        }
        public static IEnumerator WaitUntil(Func<bool> condition, TimeSpan timeout)
        {
            // new WaitUntil(() => task.IsCompleted)
            using (var timeoutToken = new CancellationTokenSource())
            using (var disposable = timeoutToken.CancelAfterSlim(timeout))
            yield return UniTask.WaitUntil(condition, cancellationToken: timeoutToken.Token).ToCoroutine();
        }
        public static IEnumerator Wait(TimeSpan duration)
        {
            yield return UniTask.Delay(duration).ToCoroutine();
        }
        public static IEnumerator TimeoutCoroutine(this UniTask task, TimeSpan timeout)
        {
            // task.Status ==
            yield return task.Timeout(timeout).ToCoroutine();
        }
        public static IEnumerator TimeoutCoroutine(this Task task, TimeSpan timeout)
        {
            if (task.IsCompleted)
                yield break;

            yield return task.AsUniTask().Timeout(timeout).ToCoroutine();
        }
        public static IEnumerator TimeoutCoroutine<T>(this IFuture<T> future, TimeSpan timeout)
        {
            yield return future.AsUniTask().Timeout(timeout).ToCoroutine();
        }
    }
}