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
        public static IFuture<T> Register<T>(this IFuture<T> future, CancellationToken cancellationToken)
        {
            cancellationToken.Register(future.Cancel);
            return future;
        }
    }
}
