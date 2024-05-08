using System;
using System.Threading;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T> : IDisposable
    {
        public readonly string Url;

        private event Action<T>         OnLoadedFromMemoryCache;
        private event Action            OnLoadingFromDiskCache;
        private event Action<T>         OnLoadedFromDiskCache;
        private event Action            OnLoadingFromSource;
        private event Action<T>         OnLoadedFromSource;
        private event Action<T>         OnLoaded;
        private event Action<Exception> OnFailedToLoad;
        private event Action<bool>      OnCompleted;
        private event Action            OnCancelled;

        private bool                    cleared   = false;
        private T                       value     = default;
        private Exception               exception = default;

        public bool IsCancelled => Status == FutureStatus.Canceled;
        public bool IsLoaded => Status == FutureStatus.LoadedFromMemoryCache
            || Status == FutureStatus.LoadedFromDiskCache
            || Status == FutureStatus.LoadedFromSource;
        public bool IsCompleted => Status == FutureStatus.LoadedFromMemoryCache
            || Status == FutureStatus.LoadedFromDiskCache
            || Status == FutureStatus.LoadedFromSource
            || Status == FutureStatus.FailedToLoad;
        public bool IsInProgress => Status == FutureStatus.Initialized
            || Status == FutureStatus.LoadingFromDiskCache
            || Status == FutureStatus.LoadingFromSource;
        public FutureStatus Status { get; private set; } = FutureStatus.Initialized;

        internal Future(string url, CancellationToken cancellationToken)
        {
            Url = url;
            cancellationToken.Register(Cancel);
        }
        ~Future() => Dispose();
        
        internal Future<T> PassEvents(Future<T> to)
        {
            OnLoadedFromMemoryCache += (v) => to.Loaded(v, FutureLoadedFrom.MemoryCache);
            OnLoadingFromDiskCache  += ( ) => to.Loading(FutureLoadingFrom.DiskCache);
            OnLoadedFromDiskCache   += (v) => to.Loaded(v, FutureLoadedFrom.DiskCache);
            OnLoadingFromSource     += ( ) => to.Loading(FutureLoadingFrom.Source);
            OnLoadedFromSource      += (v) => to.Loaded(v, FutureLoadedFrom.Source);
            OnFailedToLoad          += to.FailToLoad;
            OnCancelled             += to.Cancel;

            return this;
        }
        internal void Loading(FutureLoadingFrom loadingFrom)
        {
            if (cleared || IsCancelled) return;
            Status = loadingFrom switch
            {
                FutureLoadingFrom.DiskCache => FutureStatus.LoadingFromDiskCache,
                FutureLoadingFrom.Source    => FutureStatus.LoadingFromSource,
                _ => throw new ArgumentException($"Unsupported FutureLoadingFrom with value = '{loadingFrom}' in LoadingFrom")
            };
            var onLoadingEvent = loadingFrom switch
            {
                FutureLoadingFrom.DiskCache => OnLoadingFromDiskCache,
                FutureLoadingFrom.Source    => OnLoadingFromSource,
                _ => throw new ArgumentException($"Unsupported FutureLoadingFrom with value = '{loadingFrom}' in LoadingFrom")
            };

            if (ImageLoader.settings.debugLevel <= DebugLevel.Log)
                Debug.Log($"[ImageLoader] Loading: {Url}, from: {loadingFrom}");

            onLoadingEvent?.Invoke();
        }
        internal void Loaded(T value, FutureLoadedFrom loadedFrom)
        {
            if (cleared || IsCancelled) return;
            this.value = value;
            Status = loadedFrom switch
            {
                FutureLoadedFrom.MemoryCache => FutureStatus.LoadedFromMemoryCache,
                FutureLoadedFrom.DiskCache   => FutureStatus.LoadedFromDiskCache,
                FutureLoadedFrom.Source      => FutureStatus.LoadedFromSource,
                _ => throw new ArgumentException($"Unsupported FutureLoadedFrom with value = '{loadedFrom}' in Loaded")
            };
            var onLoadedEvent = loadedFrom switch
            {
                FutureLoadedFrom.MemoryCache => OnLoadedFromMemoryCache,
                FutureLoadedFrom.DiskCache => OnLoadedFromDiskCache,
                FutureLoadedFrom.Source => OnLoadedFromSource,
                _ => throw new ArgumentException($"Unsupported FutureLoadedFrom with value = '{loadedFrom}' in Loaded")
            };

            if (ImageLoader.settings.debugLevel <= DebugLevel.Log)
                Debug.Log($"[ImageLoader] Loaded: {Url}, from: {loadedFrom}");

            onLoadedEvent?.Invoke(value);
            OnLoaded?.Invoke(value);
            OnCompleted?.Invoke(true);
            Clear();
        }
        internal void FailToLoad(Exception exception)
        {
            if (cleared || IsCancelled) return;
            this.exception = exception;
            Status = FutureStatus.FailedToLoad;

            if (ImageLoader.settings.debugLevel <= DebugLevel.Error)
                Debug.LogError(exception.Message);

            OnFailedToLoad?.Invoke(exception);
            OnCompleted?.Invoke(false);
            Clear();
        }

        private void Clear()
        {
            cleared = true;

            OnLoadedFromMemoryCache = null;
            OnLoadingFromDiskCache = null;
            OnLoadedFromDiskCache = null;
            OnLoadingFromSource = null;
            OnLoadedFromSource = null;
            OnLoaded = null;
            OnFailedToLoad = null;
            OnCompleted = null;
            OnCancelled = null;
        }
    }
}
