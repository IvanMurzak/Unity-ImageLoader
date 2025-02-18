using System;

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    public partial class FakeFuture<T> : IFutureInternal<T>
    {
        public void Loading(FutureLoadingFrom loadingFrom)
        {
            if (LogLevel.IsActive(DebugLevel.Trace))
                UnityEngine.Debug.Log($"FakeFuture[id={Id}] Loading from {loadingFrom}");

            EventName eventName;
            switch (loadingFrom)
            {
                case FutureLoadingFrom.DiskCache:
                    eventName = EventName.LoadingFromDiskCache;
                    break;
                case FutureLoadingFrom.Source:
                    eventName = EventName.LoadingFromSource;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(loadingFrom), loadingFrom, null);
            };
            lock (events)
                events.Add(new EventData { name = eventName });
        }

        public void Loaded(T value, FutureLoadedFrom loadedFrom)
        {
            if (LogLevel.IsActive(DebugLevel.Trace))
                UnityEngine.Debug.Log($"FakeFuture[id={Id}] Loaded from {loadedFrom}");

            EventName eventName;
            switch (loadedFrom)
            {
                case FutureLoadedFrom.MemoryCache:
                    eventName = EventName.LoadedFromMemoryCache;
                    break;
                case FutureLoadedFrom.DiskCache:
                    eventName = EventName.LoadedFromDiskCache;
                    break;
                case FutureLoadedFrom.Source:
                    eventName = EventName.LoadedFromSource;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(loadedFrom), loadedFrom, null);
            };

            lock (events)
                events.Add(new EventData { name = eventName, value = value });
        }

        public void FailToLoad(Exception exception)
        {
            if (LogLevel.IsActive(DebugLevel.Trace))
                UnityEngine.Debug.Log($"FakeFuture[id={Id}] FailToLoad: {exception}");
            lock (events)
                events.Add(new EventData { name = EventName.Failed, value = exception });
        }
    }
}