using Cysharp.Threading.Tasks;
using System;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T>
    {
        /// <summary>
        /// Set timeout duration. If the duration reached it fails the Future with related exception
        /// </summary>
        /// <param name="duration">The timeout duration</param>
        /// <returns>Returns async Future</returns>
        public Future<T> Timeout(TimeSpan duration)
        {
            timeout = duration;

            if ((WebRequest?.isModifiable) ?? false)
                WebRequest.timeout = (int)Math.Ceiling(duration.TotalSeconds);

            if (duration <= TimeSpan.Zero)
            {
                FailToLoad(new Exception($"[ImageLoader] Future[id={id}] Timeout ({duration}): {Url}"));
                return this;
            }

            UniTask.Delay(duration)
                .ContinueWith(() => FailToLoad(new Exception($"[ImageLoader] Future[id={id}] Timeout ({duration}): {Url}")))
                .AttachExternalCancellation(CancellationToken);

            return this;
        }
    }
}
