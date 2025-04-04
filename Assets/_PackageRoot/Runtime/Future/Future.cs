﻿using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace Extensions.Unity.ImageLoader
{
    internal static class FutureMetadata
    {
        public static volatile uint idCounter = 0;
    }
    public partial class Future<T> : IFuture<T>, IFuture, IFutureInternal<T>, IDisposable
    {
        public string Url { get; }

        protected event Action<T>           OnLoadedFromMemoryCache;
        protected event Action              OnLoadingFromDiskCache;
        protected event Action<T>           OnLoadedFromDiskCache;
        protected event Action              OnLoadingFromSource;
        protected event Action<T>           OnLoadedFromSource;
        protected event Action<T>           OnLoaded;
        protected event Action<Exception>   OnFailedToLoad;
        protected event Action<bool>        OnCompleted;

        // ┌─────────────────────┬────────────────────────────────────────────────┐
        // │ Memory Leak Warning │ Subscription on OnCanceled cross references    │
        // └─────────────────────┘ with another Future                            │
        //                                                                        │
        protected event Action              OnCanceled;                        // │
        // protected       WeakAction       OnCanceled = new WeakAction();     // │
        // ───────────────────────────────────────────────────────────────────────┘

        protected readonly CancellationTokenSource cts;

        public uint Id { get; } = FutureMetadata.idCounter++;

        protected readonly List<Action<T>> consumers = new List<Action<T>>();
        protected readonly Dictionary<FutureStatus, T> placeholders = new Dictionary<FutureStatus, T>();
        protected TimeSpan timeout;
        protected bool cleared = false;
        protected bool disposeValue = false;
        protected T value = default;
        protected FutureLoadingFrom? loadingFrom = null;
        protected Exception exception = default;
        protected UnityWebRequest webRequest = null;

        public DebugLevel LogLevel { get; private set; }
        public bool UseDiskCache { get; private set; }
        public bool UseMemoryCache { get; private set; }

        UnityWebRequest IFutureInternal<T>.WebRequest => WebRequest;
        UnityWebRequest WebRequest
        {
            get => webRequest;
            set
            {
                if (webRequest != null)
                    throw new InvalidOperationException($"Future[id={Id}] WebRequest is already set");

                webRequest = value;
            }
        }

        public T Value => value;
        public bool IsCancelled => Status == FutureStatus.Canceled;
        public bool IsLoaded
            => Status == FutureStatus.LoadedFromMemoryCache
            || Status == FutureStatus.LoadedFromDiskCache
            || Status == FutureStatus.LoadedFromSource;
        public bool IsCompleted
            => Status == FutureStatus.LoadedFromMemoryCache
            || Status == FutureStatus.LoadedFromDiskCache
            || Status == FutureStatus.LoadedFromSource
            || Status == FutureStatus.FailedToLoad;
        public bool IsInProgress
            => Status == FutureStatus.Initialized
            || Status == FutureStatus.LoadingFromDiskCache
            || Status == FutureStatus.LoadingFromSource;
        public FutureStatus Status { get; private set; } = FutureStatus.Initialized;
        public CancellationToken CancellationToken => cts.Token;

        protected Future(string url, CancellationToken cancellationToken = default, DebugLevel? logLevel = null)
        {
            Url = url;
            UseDiskCache = ImageLoader.settings.useDiskCache;
            UseMemoryCache = ImageLoader.settings.useMemoryCache;
            timeout = ImageLoader.settings.timeout;
            LogLevel = logLevel ?? ImageLoader.settings.debugLevel;
            cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            if (LogLevel.IsActive(DebugLevel.Trace))
                Debug.Log($"[ImageLoader] Future[id={Id}] Created future ({typeof(T).Name})\n{url}");

            cancellationToken.Register(Cancel);
        }
        ~Future() => Dispose();

        public IFuture<T> PassEvents(IFutureInternal<T> to, bool passCancelled = true)
        {
            if (LogLevel.IsActive(DebugLevel.Trace))
                Debug.Log($"[ImageLoader] Future[id={Id}] -> Future[id={to.Id}] Subscribe on events\n{Url}");

            LoadedFromMemoryCache((v) => to.SetLoaded(v, FutureLoadedFrom.MemoryCache));
            LoadingFromDiskCache(( ) => to.Loading(FutureLoadingFrom.DiskCache));
            LoadedFromDiskCache((v) => to.SetLoaded(v, FutureLoadedFrom.DiskCache));
            LoadingFromSource(( ) => to.Loading(FutureLoadingFrom.Source));
            LoadedFromSource((v) => to.SetLoaded(v, FutureLoadedFrom.Source));
            Failed(to.FailToLoad);

            if (passCancelled)
            {
                if (Status != FutureStatus.Disposed && !IsCompleted)
                    Canceled(to.Cancel);
            }

            return this;
        }
        public IFuture<T> PassEvents<T2>(IFutureInternal<T2> to, Func<T, T2> convert, bool passCancelled = true)
        {
            if (LogLevel.IsActive(DebugLevel.Log))
                Debug.Log($"[ImageLoader] Future[id={Id}] -> Future[id={to.Id}] Subscribe on events (${typeof(T).Name} -> ${typeof(T2).Name})\n{Url}");

            LoadedFromMemoryCache((v) => to.SetLoaded(convert(v), FutureLoadedFrom.MemoryCache));
            LoadingFromDiskCache(( ) => to.Loading(FutureLoadingFrom.DiskCache));
            LoadedFromDiskCache((v) => to.SetLoaded(convert(v), FutureLoadedFrom.DiskCache));
            LoadingFromSource(( ) => to.Loading(FutureLoadingFrom.Source));
            LoadedFromSource((v) => to.SetLoaded(convert(v), FutureLoadedFrom.Source));
            Failed(to.FailToLoad);

            if (passCancelled)
            {
                if (Status != FutureStatus.Disposed && !IsCompleted)
                    Canceled(to.Cancel);
            }

            return this;
        }
        void IFutureInternal<T>.Loading(FutureLoadingFrom loadingFrom)
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

            this.loadingFrom = loadingFrom;
            Safe.Run(onLoadingEvent, LogLevel);
            ActivatePlaceholder(Status);
        }
        void IFutureInternal<T>.SetLoaded(T value, FutureLoadedFrom loadedFrom)
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

            Safe.Run(onLoadedEvent, this.value, LogLevel);
            Safe.Run(OnLoaded, this.value, LogLevel);
            FeedConsumers(this.value);
            Safe.Run(OnCompleted, true, LogLevel);
            Clear();
        }
        void IFutureInternal<T>.FailToLoad(Exception exception)
        {
            if (cleared || IsCancelled) return;
            if (IsCompleted || Status == FutureStatus.FailedToLoad) return;
            this.exception = exception;
            Status = FutureStatus.FailedToLoad;

            if (LogLevel.IsActive(DebugLevel.Error))
                Debug.LogError(exception.Message);

            Safe.Run(OnFailedToLoad, exception, LogLevel);
            ActivatePlaceholder(Status);
            Safe.Run(OnCompleted, false, LogLevel);
            Clear();
        }
        void IFutureInternal<T>.SetTimeout(TimeSpan duration) => timeout = duration;
        void ActivatePlaceholder(FutureStatus status)
        {
            lock (placeholders)
            {
                if (placeholders.TryGetValue(status, out var placeholder))
                    FeedConsumers(placeholder);
            }
        }
        void FeedConsumers(T value)
        {
            lock (consumers)
            {
                foreach (var setter in consumers)
                {
                    if (LogLevel.IsActive(DebugLevel.Trace))
                        Debug.Log($"[ImageLoader] Future[id={Id}] Feed consumer\n{Url}");
                    Safe.Run(setter, value, LogLevel);
                }
            }
        }

        protected virtual void Clear()
        {
            if (LogLevel.IsActive(DebugLevel.Trace))
                Debug.Log($"[ImageLoader] Future[id={Id}] Cleared\n{Url}");
            cleared = true;

            // lock (placeholders)
            //     placeholders.Clear();
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
