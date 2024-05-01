using System.Threading;

namespace Extensions.Unity.ImageLoader
{
    public static partial class FutureEx
    {
        /// <summary>
        /// Register new cancellation token to cancel the Future with it
        /// </summary>
        /// <param name="cancellationToken">New cancellation token</param>
        /// <returns>Returns async Future</returns>
        public static Future<T> Register<T>(this Future<T> future, CancellationToken cancellationToken)
        {
            cancellationToken.Register(future.Cancel);
            return future;
        }
    }
}
