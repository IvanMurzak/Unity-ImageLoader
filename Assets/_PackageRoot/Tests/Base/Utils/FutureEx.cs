namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    public static class FutureEx
    {
        public static FutureListener<T> ToFutureListener<T>(this IFuture<T> future, bool ignoreLoadingWhenLoaded = false, bool ignorePlaceholder = false)
            => new FutureListener<T>(future, ignoreLoadingWhenLoaded: ignoreLoadingWhenLoaded, ignorePlaceholder: ignorePlaceholder);
    }
}