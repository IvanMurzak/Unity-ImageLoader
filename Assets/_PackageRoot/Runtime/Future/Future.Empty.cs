using System;
using System.Threading;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T>
    {
        /// <summary>
        /// Create and return empty Future<T> instance with loading status
        /// </summary>
        public static Future<T> EmptyLoading(string url = null, FutureLoadingFrom from = FutureLoadingFrom.Source, T value = default)
        {
            var future = new Future<T>(url, CancellationToken.None, true);
                future.Loading(from);
            return future;
        }

        /// <summary>
        /// Create and return empty Future<T> instance with loaded status
        /// </summary>
        public static Future<T> EmptyLoaded(string url = null, FutureLoadedFrom from = FutureLoadedFrom.Source, T value = default)
        {
            var future = new Future<T>(url, CancellationToken.None, true);
                future.Loaded(value, from);
            return future;
        }

        /// <summary>
        /// Create and return empty Future<T> instance with failed to load status
        /// </summary>
        public static Future<T> EmptyFailedToLoad(string url = null, Exception exception = null)
        {
            var future = new Future<T>(url, CancellationToken.None, true);
                future.FailToLoad(exception);
            return future;
        }

        /// <summary>
        /// Create and return empty Future<T> instance with canceled status
        /// </summary>
        public static Future<T> EmptyCanceled(string url = null)
        {
            var future = new Future<T>(url, CancellationToken.None, true);
                future.Cancel();
            return future;
        }
    }
}
