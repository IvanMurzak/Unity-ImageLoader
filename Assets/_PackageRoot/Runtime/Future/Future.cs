using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace Extensions.Unity.ImageLoader
{
    public interface IFuture
    {
        string Url { get; }
        bool IsCancelled { get; }
        bool IsLoaded { get; }
        bool IsCompleted { get; }
        bool IsInProgress { get; }
        FutureStatus Status { get; }
        CancellationToken CancellationToken { get; }
        void Cancel();
    }
    public partial class Future<T> : IFuture, IDisposable
    {
        private static int idCounter = 0;

        public string Url { get; }

        private event Action<T>           OnLoadedFromMemoryCache;
        private event Action              OnLoadingFromDiskCache;
        private event Action<T>           OnLoadedFromDiskCache;
        private event Action              OnLoadingFromSource;
        private event Action<T>           OnLoadedFromSource;
        private event Action<T>           OnLoaded;
        private event Action<Exception>   OnFailedToLoad;
        private event Action<bool>        OnCompleted;
        private event Action              OnCanceled;
        private event Action<Future<T>>   OnDispose;

        private readonly CancellationTokenSource cts;
        private readonly bool muteLogs;

        internal readonly int id = idCounter++;

        public bool UseDiskCache { get; private set; }
        public bool UseMemoryCache { get; private set; }
        private TimeSpan timeout;
        private bool cleared = false;
        private bool disposeValue = false;
        private T value = default;
        private Exception exception = default;

        internal UnityWebRequest WebRequest { get; private set; }

        public T Value => value;
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
        public CancellationToken CancellationToken => cts.Token;

        internal Future(string url, CancellationToken cancellationToken, bool muteLogs = false)
        {
            Url = url;
            cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            this.muteLogs = muteLogs;
            UseDiskCache = ImageLoader.settings.useDiskCache;
            UseMemoryCache = ImageLoader.settings.useMemoryCache;
            timeout = ImageLoader.settings.timeout;

            cancellationToken.Register(Cancel);
        }
        ~Future() => Dispose();

        internal Future<T> PassEvents(Future<T> to, bool passCancelled = true, bool passDisposed = false)
        {
            LoadedFromMemoryCache((v) => to.Loaded(v, FutureLoadedFrom.MemoryCache));
            LoadingFromDiskCache (( ) => to.Loading(FutureLoadingFrom.DiskCache));
            LoadedFromDiskCache  ((v) => to.Loaded(v, FutureLoadedFrom.DiskCache));
            LoadingFromSource    (( ) => to.Loading(FutureLoadingFrom.Source));
            LoadedFromSource     ((v) => to.Loaded(v, FutureLoadedFrom.Source));
            Failed               (to.FailToLoad);

            if (passCancelled)
                Canceled(to.Cancel);

            if (passDisposed)
                Disposed(future => to.Dispose());

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

            if (ImageLoader.settings.debugLevel <= DebugLevel.Log && !muteLogs)
                Debug.Log($"[ImageLoader] Future[id={id}] Loading: {Url}, from: {loadingFrom}");

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

            if (ImageLoader.settings.debugLevel <= DebugLevel.Log && !muteLogs)
                Debug.Log($"[ImageLoader] Future[id={id}] Loaded: {Url}, from: {loadedFrom}");

            onLoadedEvent?.Invoke(value);
            OnLoaded?.Invoke(value);
            OnCompleted?.Invoke(true);
            Clear();
        }
        internal void FailToLoad(Exception exception)
        {
            if (cleared || IsCancelled) return;
            if (IsCompleted || Status == FutureStatus.FailedToLoad) return;
            this.exception = exception;
            Status = FutureStatus.FailedToLoad;

            if (ImageLoader.settings.debugLevel <= DebugLevel.Error && !muteLogs)
                Debug.LogError(exception.Message);

            cts.Cancel();
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
            OnCanceled = null;

            WebRequest?.Abort();
        }

        public override string ToString() => Url;
        public override int GetHashCode() => Url.GetHashCode();
        public override bool Equals(object obj) => obj is IFuture future && future.Url == Url;
    }
}
