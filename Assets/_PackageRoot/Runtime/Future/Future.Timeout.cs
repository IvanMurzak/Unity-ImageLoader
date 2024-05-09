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
        public static Future<T> Timeout<T>(this Future<T> future, TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero)
            {
                future.FailToLoad(new Exception($"[ImageLoader] Timeout ({duration}): {future.Url}"));
                return future;
            }
            UniTask.Delay(duration).ContinueWith(() =>
            {
                if (!future.IsCompleted)
                    future.FailToLoad(new Exception($"[ImageLoader] Timeout ({duration}): {future.Url}"));
            });
            return future;
        }
    }
}
