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
    public enum FutureLoadedFrom
    {
        MemoryCache, DiskCache, Source, FailedToLoad
    }
    public enum FutureLoadingFrom
    {
        DiskCache, Source
    }
}
