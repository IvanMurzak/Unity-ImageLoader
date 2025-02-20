using Cysharp.Threading.Tasks;
using System;

namespace Extensions.Unity.ImageLoader
{
    public static partial class FutureEx
    {
        /// <summary>
        /// Set timeout duration. If the duration reached it fails the Future with related exception
        /// </summary>
        /// <param name="duration">The timeout duration</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<T> Timeout<T>(this IFuture<T> future, TimeSpan duration)
        {
            var internalFuture = (IFutureInternal<T>)future;
            internalFuture.SetTimeout(duration);

            if ((internalFuture.WebRequest?.isModifiable) ?? false)
                internalFuture.WebRequest.timeout = (int)Math.Ceiling(duration.TotalSeconds);

            if (duration <= TimeSpan.Zero)
            {
                internalFuture.FailToLoad(new Exception($"[ImageLoader] Future[id={future.Id}] Timeout ({duration}): {future.Url}"));
                return future;
            }

            UniTask.Delay(duration)
                .ContinueWith(() => internalFuture.FailToLoad(new Exception($"[ImageLoader] Future[id={future.Id}] Timeout ({duration}): {future.Url}")))
                .AttachExternalCancellation(future.CancellationToken);

            return future;
        }
    }
}
