
using System;

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    public enum EventName
    {
        LoadedFromMemoryCache,
        LoadingFromDiskCache,
        LoadedFromDiskCache,
        LoadingFromSource,
        LoadedFromSource,
        Then,
        Failed,
        Completed,
        Canceled,
        Consume
    }

    public static class EventNameEx
    {
        public static EventName ToEventName(this FutureLoadingFrom loadingFrom)
        {
            switch (loadingFrom)
            {
                case FutureLoadingFrom.DiskCache:
                    return EventName.LoadingFromDiskCache;
                case FutureLoadingFrom.Source:
                    return EventName.LoadingFromSource;
                default:
                    throw new ArgumentOutOfRangeException(nameof(loadingFrom), loadingFrom, null);
            };
        }
        public static EventName ToEventName(this FutureLoadedFrom loadedFrom)
        {
            switch (loadedFrom)
            {
                case FutureLoadedFrom.MemoryCache:
                    return EventName.LoadedFromMemoryCache;
                case FutureLoadedFrom.DiskCache:
                    return EventName.LoadedFromDiskCache;
                case FutureLoadedFrom.Source:
                    return EventName.LoadedFromSource;
                case FutureLoadedFrom.FailedToLoad:
                    return EventName.Failed;
                default:
                    throw new ArgumentOutOfRangeException(nameof(loadedFrom), loadedFrom, null);
            };
        }
    }
}