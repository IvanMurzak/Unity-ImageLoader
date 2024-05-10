namespace Extensions.Unity.ImageLoader
{
    public enum FutureStatus
    {
        Initialized,
        LoadedFromMemoryCache,
        LoadingFromDiskCache,
        LoadedFromDiskCache,
        LoadingFromSource,
        LoadedFromSource,
        FailedToLoad,
        Canceled,
        Disposed
    }
    internal enum FutureLoadedFrom
    {
        MemoryCache, DiskCache, Source, FailedToLoad
    }
    internal enum FutureLoadingFrom
    {
        DiskCache, Source
    }
}
