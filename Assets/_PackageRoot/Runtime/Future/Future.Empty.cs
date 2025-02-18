using System;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T>
    {
        /// <summary>
        /// Create and return empty Future<T> instance with loading status
        /// </summary>
        public static IFuture<T> EmptyLoading(string url = null, FutureLoadingFrom from = FutureLoadingFrom.Source, T value = default)
        {
            var future = new FutureEmpty<T>(url).SetLogLevel(DebugLevel.None);
            ((IFutureInternal<T>)future).Loading(from);
            return future;
        }

        /// <summary>
        /// Create and return empty Future<T> instance with loaded status
        /// </summary>
        public static IFuture<T> EmptyLoaded(string url = null, FutureLoadedFrom from = FutureLoadedFrom.Source, T value = default)
        {
            var future = new FutureEmpty<T>(url).SetLogLevel(DebugLevel.None);
            ((IFutureInternal<T>)future).Loaded(value, from);
            return future;
        }

        /// <summary>
        /// Create and return empty Future<T> instance with failed to load status
        /// </summary>
        public static IFuture<T> EmptyFailedToLoad(string url = null, Exception exception = null)
        {
            var future = new FutureEmpty<T>(url).SetLogLevel(DebugLevel.None);
            ((IFutureInternal<T>)future).FailToLoad(exception);
            return future;
        }

        /// <summary>
        /// Create and return empty Future<T> instance with canceled status
        /// </summary>
        public static IFuture<T> EmptyCanceled(string url = null)
        {
            var future = new FutureEmpty<T>(url).SetLogLevel(DebugLevel.None);
            future.Cancel();
            return future;
        }
    }
}
