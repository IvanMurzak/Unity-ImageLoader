﻿using System;
using System.Collections.Generic;
using System.Threading;
using Extensions.Unity.ImageLoader.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace Extensions.Unity.ImageLoader
{
    internal static class FutureMetadata
    {
        public static volatile uint idCounter = 0;
    }
    public partial class Future<T> : IFuture, IFuture<T>, IDisposable
    {

        public string Url { get; }

        private event Action<T>           OnLoadedFromMemoryCache;
        private event Action              OnLoadingFromDiskCache;
        private event Action<T>           OnLoadedFromDiskCache;
        private event Action              OnLoadingFromSource;
        private event Action<T>           OnLoadedFromSource;
        private event Action<T>           OnLoaded;
        private event Action<Exception>   OnFailedToLoad;
        private event Action<bool>        OnCompleted;
        private       WeakAction          OnCanceled = new WeakAction();
        // private event Action              OnCanceled;
        private event Action<Future<T>>   OnDispose;

        private readonly CancellationTokenSource cts;

        public uint Id { get; } = FutureMetadata.idCounter++;

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

        protected Future(string url, CancellationToken cancellationToken = default)
        {
            Url = url;
            UseDiskCache = ImageLoader.settings.useDiskCache;
            UseMemoryCache = ImageLoader.settings.useMemoryCache;
            timeout = ImageLoader.settings.timeout;
            LogLevel = ImageLoader.settings.debugLevel;
            cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            if (LogLevel.IsActive(DebugLevel.Trace))
                Debug.Log($"[ImageLoader] Future[id={Id}] Created ({typeof(T).Name})\n{url}");

            cancellationToken.Register(Cancel);
        }
        ~Future() => Dispose();

        public IFuture<T> PassEvents(IFuture<T> to, bool passCancelled = true, bool passDisposed = false)
        {
            if (LogLevel.IsActive(DebugLevel.Trace))
                Debug.Log($"[ImageLoader] Future[id={Id}] -> Future[id={to.Id}] Subscribe on events\n{Url}");

            var internalTo = (IFutureInternal<T>)to;

            LoadedFromMemoryCache((v) => internalTo.Loaded(v, FutureLoadedFrom.MemoryCache));
            LoadingFromDiskCache (( ) => internalTo.Loading(FutureLoadingFrom.DiskCache));
            LoadedFromDiskCache  ((v) => internalTo.Loaded(v, FutureLoadedFrom.DiskCache));
            LoadingFromSource    (( ) => internalTo.Loading(FutureLoadingFrom.Source));
            LoadedFromSource     ((v) => internalTo.Loaded(v, FutureLoadedFrom.Source));
            Failed               (internalTo.FailToLoad);

            if (passCancelled)
                Canceled(to.Cancel);

            if (passDisposed)
                Disposed(future => to.Dispose());

            return this;
        }
        public IFuture<T> PassEvents<T2>(IFuture<T2> to, Func<T, T2> convert, bool passCancelled = true, bool passDisposed = false)
        {
            if (LogLevel.IsActive(DebugLevel.Log))
                Debug.Log($"[ImageLoader] Future[id={Id}] -> Future[id={to.Id}] Subscribe on events (${typeof(T).Name} -> ${typeof(T2).Name})\n{Url}");

            var internalTo = (IFutureInternal<T2>)to;

            LoadedFromMemoryCache((v) => internalTo.Loaded(convert(v), FutureLoadedFrom.MemoryCache));
            LoadingFromDiskCache (( ) => internalTo.Loading(FutureLoadingFrom.DiskCache));
            LoadedFromDiskCache  ((v) => internalTo.Loaded(convert(v), FutureLoadedFrom.DiskCache));
            LoadingFromSource    (( ) => internalTo.Loading(FutureLoadingFrom.Source));
            LoadedFromSource     ((v) => internalTo.Loaded(convert(v), FutureLoadedFrom.Source));
            Failed               (internalTo.FailToLoad);

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
                Debug.Log($"[ImageLoader] Future[id={Id}] Placeholder\n{Url}");

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

            Action onLoadingEvent;
            switch (loadingFrom)
            {
                case FutureLoadingFrom.DiskCache:
                    Status = FutureStatus.LoadingFromDiskCache;
                    onLoadingEvent = OnLoadingFromDiskCache;
                    break;
                case FutureLoadingFrom.Source:
                    Status = FutureStatus.LoadingFromSource;
                    onLoadingEvent = OnLoadingFromSource;
                    break;
                default:
                    throw new ArgumentException($"Unsupported FutureLoadingFrom with value = '{loadingFrom}' in LoadingFrom");
            }

            if (LogLevel.IsActive(DebugLevel.Log))
                Debug.Log($"[ImageLoader] Future[id={Id}] Loading from: {loadingFrom}\n{Url}");

            Safe.Run(onLoadingEvent, LogLevel);
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
                Debug.Log($"[ImageLoader] Future[id={Id}] Loaded from {loadedFrom}\n{Url}");

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
