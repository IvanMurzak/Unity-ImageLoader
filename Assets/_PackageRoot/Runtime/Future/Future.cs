using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Extensions.Unity.ImageLoader.Utils;
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

        internal readonly int id = idCounter++;

        private TimeSpan timeout;
        private List<Action<T, Sprite>> setters = new List<Action<T, Sprite>>();
        private bool cleared = false;
        private bool disposeValue = false;
        private T value = default;
        private Exception exception = default;

        public DebugLevel LogLevel { get; private set; }
        public bool UseDiskCache { get; private set; }
        public bool UseMemoryCache { get; private set; }
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

        protected Future(string url, CancellationToken cancellationToken)
        {
            Url = url;
            cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            UseDiskCache = ImageLoader.settings.useDiskCache;
            UseMemoryCache = ImageLoader.settings.useMemoryCache;
            timeout = ImageLoader.settings.timeout;
            LogLevel = ImageLoader.settings.debugLevel;

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
        internal Future<T> PassEvents<T2>(Future<T2> to, Func<T, T2> convert, bool passCancelled = true, bool passDisposed = false)
        {
            LoadedFromMemoryCache((v) => to.Loaded(convert(v), FutureLoadedFrom.MemoryCache));
            LoadingFromDiskCache (( ) => to.Loading(FutureLoadingFrom.DiskCache));
            LoadedFromDiskCache  ((v) => to.Loaded(convert(v), FutureLoadedFrom.DiskCache));
            LoadingFromSource    (( ) => to.Loading(FutureLoadingFrom.Source));
            LoadedFromSource     ((v) => to.Loaded(convert(v), FutureLoadedFrom.Source));
            Failed               (to.FailToLoad);

            if (passCancelled)
                Canceled(to.Cancel);

            if (passDisposed)
                Disposed(future => to.Dispose());

            return this;
        }
        internal void Placeholder(Sprite placeholder)
        {
            if (cleared || IsCancelled) return;

            if (LogLevel.IsActive(DebugLevel.Log))
                Debug.Log($"[ImageLoader] Future[id={id}] Placeholder: {Url}");

            // UniTask.ReturnToMainThread()
            // if (UnityMainThreadDispatcher.IsMainThread)
            // {
            //     OnLoadedFromMemoryCache?.Invoke(placeholder);
            //     OnLoadedFromDiskCache?.Invoke(placeholder);
            //     OnLoadedFromSource?.Invoke(placeholder);
            //     OnLoaded?.Invoke(placeholder);
            //     OnCompleted?.Invoke(true);
            //     Clear();
            // }
            // else
            // {
            //     UniTask.SwitchToMainThread();
            //     Placeholder(placeholder);
            // }

            OnLoadedFromMemoryCache += (v) => { };
            OnLoadingFromDiskCache += ( ) => { };
            OnLoadedFromDiskCache += (v) => { };
            OnLoadingFromSource += ( ) => { };
            OnLoadedFromSource += (v) => { };
        }
        internal void Loading(FutureLoadingFrom loadingFrom)
        {
            if (cleared || IsCancelled) return;

            switch(loadingFrom)
            {
                case FutureLoadingFrom.DiskCache:
                    Status = FutureStatus.LoadingFromDiskCache;
                    Safe.Run(OnLoadingFromDiskCache, LogLevel);
                    break;
                case FutureLoadingFrom.Source:
                    Status = FutureStatus.LoadingFromSource;
                    Safe.Run(OnLoadingFromSource, LogLevel);;
                    break;
                default:
                    throw new ArgumentException($"Unsupported FutureLoadingFrom with value = '{loadingFrom}' in LoadingFrom");
            }

            Action onLoadingEvent;
            switch (loadingFrom)
            {
                case FutureLoadingFrom.DiskCache:
                    onLoadingEvent = OnLoadingFromDiskCache;
                    break;
                case FutureLoadingFrom.Source:
                    onLoadingEvent = OnLoadingFromSource;
                    break;
                default:
                    throw new ArgumentException($"Unsupported FutureLoadingFrom with value = '{loadingFrom}' in LoadingFrom");
            }

            if (LogLevel.IsActive(DebugLevel.Log))
                Debug.Log($"[ImageLoader] Future[id={id}] Loading: {Url}, from: {loadingFrom}");

            Safe.Run(onLoadingEvent, LogLevel);;
        }
        internal void Loaded(T value, FutureLoadedFrom loadedFrom)
        {
            if (cleared || IsCancelled) return;

            this.value = value;

            Action<T> onLoadedEvent;
            switch(loadedFrom)
            {
                case FutureLoadedFrom.MemoryCache:
                    Status = FutureStatus.LoadedFromMemoryCache;
                    onLoadedEvent = OnLoadedFromMemoryCache;
                    break;
                case FutureLoadedFrom.DiskCache:
                    Status = FutureStatus.LoadedFromDiskCache;
                    onLoadedEvent = OnLoadedFromDiskCache;
                    break;
                case FutureLoadedFrom.Source:
                    Status = FutureStatus.LoadedFromSource;
                    onLoadedEvent = OnLoadedFromSource;
                    break;
                default:
                    throw new ArgumentException($"Unsupported FutureLoadedFrom with value = '{loadedFrom}' in Loaded");
            }

            if (LogLevel.IsActive(DebugLevel.Log))
                Debug.Log($"[ImageLoader] Future[id={id}] Loaded: {Url}, from: {loadedFrom}");

            Safe.Run(onLoadedEvent, value, LogLevel);
            Safe.Run(OnLoaded, value, LogLevel);
            Safe.Run(OnCompleted, true, LogLevel);;
            Clear();
        }
        internal void FailToLoad(Exception exception)
        {
            if (cleared || IsCancelled) return;
            if (IsCompleted || Status == FutureStatus.FailedToLoad) return;
            this.exception = exception;
            Status = FutureStatus.FailedToLoad;

            if (LogLevel.IsActive(DebugLevel.Error))
                Debug.LogError(exception.Message);

            Safe.Run(OnFailedToLoad, exception, LogLevel); // 2 Original order
            Safe.Run(OnCompleted, false, LogLevel);;       // 3 Original order
            Safe.Run(cts.Cancel, LogLevel);                // 1 Original order
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
        public override bool Equals(object obj) => obj != null && obj is IFuture future && future.Url == Url;
    }
}
