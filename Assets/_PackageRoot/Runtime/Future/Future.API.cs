﻿using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T> : IDisposable
    {
        /// <summary>
        /// When the it is time to set data into any consumer. Such as setting placeholder or final loaded subject
        /// </summary>
        /// <param name="consumer">Consumer (setter) function</param>
        /// <param name="replace">If true, clear all existed consumer and replace them with this new one</param>
        /// <returns>Returns the Future instance</returns>
        public IFuture<T> Consume(Action<T> consumer, bool replace = false)
        {
            if (IsLoaded)
            {
                Safe.Run(consumer, value, LogLevel);
                return this;
            }
            else if (Status == FutureStatus.LoadingFromDiskCache || Status == FutureStatus.LoadingFromSource)
            {
                lock (placeholders)
                    if (placeholders.TryGetValue(Status, out var placeholder))
                        Safe.Run(consumer, placeholder, LogLevel);
            }
            else if (Status == FutureStatus.FailedToLoad || Status == FutureStatus.Canceled)
            {
                lock (placeholders)
                    if (placeholders.TryGetValue(Status, out var placeholder))
                        Safe.Run(consumer, placeholder, LogLevel);
                return this;
            }

            lock (consumers)
            {
                if (replace)
                    consumers.Clear();
                consumers.Add(consumer);
            }
            return this;
        }

        /// <summary>
        /// When is loaded successfully from any source
        /// </summary>
        /// <param name="action">action to execute on the event</param>
        /// <returns>Returns the Future instance</returns>
        public IFuture<T> Loaded(Action<T> action)
        {
            if (IsLoaded)
            {
                Safe.Run(action, value, LogLevel);
                return this;
            }
            OnLoaded += action;
            return this;
        }

        /// <summary>
        /// When the image is failed to load from any source
        /// </summary>
        /// <param name="action">action to execute on the event</param>
        /// <returns>Returns the Future instance</returns>
        public IFuture<T> Failed(Action<Exception> action)
        {
            if (Status == FutureStatus.FailedToLoad)
            {
                Safe.Run(action, exception, LogLevel);
                return this;
            }
            OnFailedToLoad += action;
            return this;
        }

        /// <summary>
        /// When the image loading process is completed (loaded, failed or canceled)
        /// </summary>
        /// <param name="action">action to execute on the event, the bool value represents success (true means loaded, false means not loaded)</param>
        /// <returns>Returns the Future instance</returns>
        public IFuture<T> Completed(Action<bool> action)
        {
            if (IsCompleted || IsCancelled)
            {
                Safe.Run(action, IsLoaded, LogLevel);
                return this;
            }
            OnCompleted += action;
            return this;
        }

        /// <summary>
        /// When the image started to load from memory cache. Also, it will be called when the image is already loaded from memory cache.
        /// <param name="action">action to execute on the event</param>
        /// <returns>Returns the Future instance</returns>
        /// </summary>
        public IFuture<T> LoadedFromMemoryCache(Action<T> action)
        {
            if (Status == FutureStatus.LoadedFromMemoryCache)
            {
                Safe.Run(action, value, LogLevel);
                return this;
            }
            OnLoadedFromMemoryCache += action;
            return this;
        }

        /// <summary>
        /// When the image started to load from disk cache. Also, it will be called when the image is already loaded from disk cache.
        /// </summary>
        /// <param name="action">action to execute on the event</param>
        /// <param name="ignoreLoaded">Ignore the event if the image is already loaded</param>
        /// <returns>Returns the Future instance</returns>
        public IFuture<T> LoadingFromDiskCache(Action action, bool ignoreLoaded = false)
        {
            if (Status == FutureStatus.LoadedFromDiskCache)
            {
                if (ignoreLoaded)
                    return this; // ignore event

                Safe.Run(action, LogLevel);
                return this;
            }
            if (loadingFrom == FutureLoadingFrom.DiskCache)
            {
                Safe.Run(action, LogLevel);
                return this;
            }
            OnLoadingFromDiskCache += action;
            return this;
        }

        /// <summary>
        /// When the image successfully loaded from disk cache
        /// </summary>
        /// <param name="action">action to execute on the event</param>
        /// <returns>Returns the Future instance</returns>
        public IFuture<T> LoadedFromDiskCache(Action<T> action)
        {
            if (Status == FutureStatus.LoadedFromDiskCache)
            {
                Safe.Run(action, value, LogLevel);
                return this;
            }
            OnLoadedFromDiskCache += action;
            return this;
        }

        /// <summary>
        /// When the image started to load from source. Also, it will be called when the image is already loaded from source.
        /// </summary>
        /// <param name="action">action to execute on the event</param>
        /// <param name="ignoreLoaded">Ignore the event if the image is already loaded</param>
        /// <returns>Returns the Future instance</returns>
        public IFuture<T> LoadingFromSource(Action action, bool ignoreLoaded = false)
        {
            if (Status == FutureStatus.LoadedFromSource)
            {
                if (ignoreLoaded)
                    return this; // ignore event

                Safe.Run(action, LogLevel);
                return this;
            }
            if (loadingFrom == FutureLoadingFrom.Source)
            {
                Safe.Run(action, LogLevel);
                return this;
            }
            OnLoadingFromSource += action;
            return this;
        }

        /// <summary>
        /// When the image successfully loaded from source
        /// </summary>
        /// <param name="action">action to execute on the event</param>
        /// <returns>Returns the Future instance</returns>
        public IFuture<T> LoadedFromSource(Action<T> action)
        {
            if (Status == FutureStatus.LoadedFromSource)
            {
                Safe.Run(action, value, LogLevel);
                return this;
            }
            OnLoadedFromSource += action;
            return this;
        }

        /// <summary>
        /// When the image loading was canceled by calling Cancel
        /// </summary>
        /// <param name="action">action to execute on the event</param>
        /// <returns>Returns the Future instance</returns>
        public IFuture<T> Canceled(Action action)
        {
            if (IsCancelled)
            {
                if (LogLevel.IsActive(DebugLevel.Log))
                    Debug.Log($"[ImageLoader] Future[id={Id}] Canceled. Status={Status}\n{Url}");
                Safe.Run(action, LogLevel);
                return this;
            }
            if (cleared)
            {
                if (LogLevel.IsActive(DebugLevel.Warning))
                    Debug.LogWarning($"[ImageLoader] Future[id={Id}] Canceled event is not set because Future is cleared. Status={Status}\n{Url}");
                return this;
            }
            OnCanceled += action;
            return this;
        }

        /// <summary>
        /// Cancel loading process. Ignores if the loading process is already completed or canceled.
        /// </summary>
        public virtual void Cancel()
        {
            if (IsLoaded)
            {
                if (LogLevel.IsActive(DebugLevel.Warning))
                    Debug.LogWarning($"[ImageLoader] Future[id={Id}] Can't cancel. Task is already loaded. Status={Status}\n{Url}");
                return;
            }
            if (IsCancelled)
            {
                if (LogLevel.IsActive(DebugLevel.Warning))
                    Debug.LogWarning($"[ImageLoader] Future[id={Id}] Can't cancel. Task is already canceled. Status={Status}\n{Url}");
                return;
            }
            if (cleared)
            {
                if (LogLevel.IsActive(DebugLevel.Warning))
                    Debug.LogWarning($"[ImageLoader] Future[id={Id}] Can't cancel. Task is already cleared. Status={Status}\n{Url}");
                return;
            }
            if (LogLevel.IsActive(DebugLevel.Log))
                Debug.Log($"[ImageLoader] Future[id={Id}] Cancel\n{Url}");
            Status = FutureStatus.Canceled;
            if (Safe.RunCancel(cts, LogLevel))
            {
                Safe.Run(OnCanceled, LogLevel);
                ActivatePlaceholder(Status);
                Safe.Run(OnCompleted, IsLoaded, LogLevel);
            }
            Clear();
        }

        /// <summary>
        /// Convert the Future<typeparamref name="T"/> instance to a Future<Reference<typeparamref name="T"/>> instance
        /// </summary>
        /// <returns>Returns Future<Reference<T>></returns>
        public IFuture<Reference<T>> AsReference(DebugLevel logLevel = DebugLevel.Trace)
        {
            var url = Url;
            var futureRef = new FutureReference<T>(url, cts.Token, logLevel);
            var weakReference = new WeakReference<IFutureInternal<Reference<T>>>(futureRef, trackResurrection: false);

            LoadedFromMemoryCache(obj =>
            {
                if (weakReference.TryGetTarget(out var future))
                    future.SetLoaded(new Reference<T>(url, obj), FutureLoadedFrom.MemoryCache);
            });
            LoadingFromDiskCache(() =>
            {
                if (weakReference.TryGetTarget(out var future))
                    future.Loading(FutureLoadingFrom.DiskCache);
            });
            LoadedFromDiskCache(obj =>
            {
                if (weakReference.TryGetTarget(out var future))
                    future.SetLoaded(new Reference<T>(url, obj), FutureLoadedFrom.DiskCache);
            });
            LoadingFromSource(() =>
            {
                if (weakReference.TryGetTarget(out var reference))
                    reference.Loading(FutureLoadingFrom.Source);
            });
            LoadedFromSource(obj =>
            {
                if (weakReference.TryGetTarget(out var future))
                    future.SetLoaded(new Reference<T>(url, obj), FutureLoadedFrom.Source);
            });
            Failed(e =>
            {
                if (weakReference.TryGetTarget(out var future))
                    future.FailToLoad(e);
            });
            Canceled(() =>
            {
                if (weakReference.TryGetTarget(out var future))
                {
                    if (future.Status != FutureStatus.Disposed && !future.IsCompleted && !future.IsCancelled)
                        future.Cancel();
                }
            });

            // ┌─────────┬────────────────────────────────────────────────────────────────────────┐
            // │ WARNING │ It creates cross reference between two Future instances                │
            // └─────────┘ Which doesn't let to dispose non of them until the other one is        │
            // disposed explicitly. Which doesn't let to dispose non of them until the other one  │
            // is disposed explicitly.                                                            │
            // ┌──────────┬───────────────────────────────────────────────────────────────────────┤
            // │ SOLUTION │ I am using WeakReference for storing reference on the Canceled event. │
            // └──────────┘ It has another drawback - without strong reference on the event       │
            // somewhere outside it would be Disposed by Garbage Collector at some point.         │
            // As a result, the event may not fire in that case specific case.                    │
            futureRef.Canceled(Cancel);                                                        // │
            // ───────────────────────────────────────────────────────────────────────────────────┘

            return futureRef;
        }

        /// <summary>
        /// Dispose the Future instance and all its references and resources. It will also cancel the loading process if it is ongoing.
        /// </summary>
        public virtual void Dispose()
        {
            if (Status == FutureStatus.Disposed) return;

            if (LogLevel.IsActive(DebugLevel.Trace))
                Debug.Log($"[ImageLoader] Future[id={Id}] Disposing\n{Url}");

            if (!cleared && !IsCancelled)
            {
                if (Safe.RunCancel(cts, LogLevel))
                    Safe.Run(OnCanceled, LogLevel);
            }
            OnCanceled = null;
            Status = FutureStatus.Disposed;

            if (!cleared)
                Clear();

            if (disposeValue && value is IDisposable disposable)
                disposable?.Dispose();

            value = default;
            exception = default;

            Safe.Run(cts.Dispose, LogLevel);
            WebRequest?.Dispose();
            webRequest = null;

            if (LogLevel.IsActive(DebugLevel.Trace))
                Debug.Log($"[ImageLoader] Future[id={Id}] Disposed\n{Url}");
        }

        /// <summary>
        /// Helpful function to forget the compilation warning about not awaiting the task
        /// </summary>
        public void Forget()
        {
            var awaiter = GetAwaiter();
            if (awaiter.IsCompleted)
            {
                try
                {
                    if (Status == FutureStatus.Canceled)
                        return;

                    awaiter.GetResult();
                }
                catch (TaskCanceledException)
                {
                    // ignore
                }
                catch (Exception ex)
                {
                    if (LogLevel.IsActive(DebugLevel.Exception))
                        Debug.LogException(ex);
                }
            }
            else
            {
                awaiter.OnCompleted(() =>
                {
                    try
                    {
                        if (Status == FutureStatus.Canceled)
                            return;

                        awaiter.GetResult();
                    }
                    catch (TaskCanceledException)
                    {
                        // ignore
                    }
                    catch (Exception ex)
                    {
                        if (LogLevel.IsActive(DebugLevel.Exception))
                            Debug.LogException(ex);
                    }
                });
            }
        }
        public UniTask<T> AsUniTask()
        {
            if (IsCompleted)
                return UniTask.FromResult(value);

            var taskCompletionSource = new UniTaskCompletionSource<T>();

            Loaded(value => taskCompletionSource.TrySetResult(value));
            Failed(exception => taskCompletionSource.TrySetException(exception));
            Canceled(() => taskCompletionSource.TrySetCanceled());

            return taskCompletionSource.Task;
        }
        public Task<T> AsTask()
        {
            if (IsCompleted)
                return Task.FromResult(value);

            var taskCompletionSource = new TaskCompletionSource<T>();

            Loaded(taskCompletionSource.SetResult);
            Failed(taskCompletionSource.SetException);
            Canceled(taskCompletionSource.SetCanceled);

            return taskCompletionSource.Task;
        }
        public IEnumerator AsCoroutine(Action<T> resultHandler = null, Action<Exception> exceptionHandler = null)
        {
            if (IsCompleted)
            {
                resultHandler?.Invoke(value);
                yield break;
            }
            yield return AsUniTask().ToCoroutine(resultHandler, exceptionHandler);
        }
        public FutureAwaiter<T> GetAwaiter() => new FutureAwaiter<T>(AsTask().GetAwaiter());
    }
}
