namespace Extensions.Unity.ImageLoader
{
    public enum FutureStatus
    {
        Initialized = 0,
        LoadedFromMemoryCache = 1,
        LoadingFromDiskCache = 2,
        LoadedFromDiskCache = 3,
        LoadingFromSource = 4,
        LoadedFromSource = 5,
        FailedToLoad = 6,
        Canceled = 7,
        Disposed = 8
    }
    public enum FutureLoadedFrom
    {
        MemoryCache = 1,
        DiskCache = 3,
        Source = 5,
        FailedToLoad = 6
    }
    public enum FutureLoadingFrom
    {
        DiskCache = 2,
        Source = 4
    }
    public enum PlaceholderTrigger
    {
        LoadingFromDiskCache = 2,
        LoadingFromSource = 4,
        FailedToLoad = 6,
        Canceled = 7,
    }
}
