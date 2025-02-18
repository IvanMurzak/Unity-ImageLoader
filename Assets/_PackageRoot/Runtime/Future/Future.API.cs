﻿using Cysharp.Threading.Tasks;
using Extensions.Unity.ImageLoader.Utils;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T> : IDisposable
    {
        /// <summary>
        /// When the image is loaded successfully from any source
        /// </summary>
        /// <param name="action">action to execute on the event</param>
        /// <returns>Returns the Future instance</returns>
        public IFuture<T> Then(Action<T> action)
        {
            if (IsLoaded)
            {
                action(value);
                return this;
            }
            OnLoaded += action;
            return this;
        }

        /// <summary>
        /// When the image is loaded successfully from any source
        /// </summary>
        /// <param name="action">action to execute on the event</param>
        /// <returns>Returns the Future instance</returns>
        // public IFuture<T> ThenSet<C>(Action<C, T> action, C consumer)
        // {
        //     if (IsLoaded)
        //     {
        //         action(consumer, value);
        //         return this;
        //     }
        //     OnLoaded += action;
        //     return this;
        // }

        /// <summary>
        /// When the image is failed to load from any source
        /// </summary>
        /// <param name="action">action to execute on the event</param>
        /// <returns>Returns the Future instance</returns>
        public IFuture<T> Failed(Action<Exception> action)
        {
            if (Status == FutureStatus.FailedToLoad)
            {
                action(exception);
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
            if (IsCompleted)
            {
                Safe.Run(action, IsLoaded, LogLevel);
                return this;
            }
            OnCompleted += action;
            return this;
        }
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
        /// When the image started to load from disk cache
        /// </summary>
        /// <param name="action">action to execute on the event</param>
        /// <returns>Returns the Future instance</returns>
        public IFuture<T> LoadingFromDiskCache(Action action)
        {
            if (Status == FutureStatus.LoadingFromDiskCache || Status == FutureStatus.LoadedFromDiskCache)
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
        /// When the image started to load from source
        /// </summary>
        /// <param name="action">action to execute on the event</param>
        /// <returns>Returns the Future instance</returns>
        public IFuture<T> LoadingFromSource(Action action)
        {
            if (Status == FutureStatus.LoadingFromSource || Status == FutureStatus.LoadedFromSource)
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
                    Debug.Log($"[ImageLoader] Future[id={Id}] Canceled\n{Url}");
                Safe.Run(action, LogLevel);
                return this;
            }
            if (OnCanceled != null)
                OnCanceled += action;
            return this;
        }

        /// <summary>
        /// Cancel loading process
        /// </summary>
        public virtual void Cancel()
        {
            if (cleared || IsCancelled)
            {
                if (LogLevel.IsActive(DebugLevel.Warning))
                    Debug.LogWarning($"[ImageLoader] Future[id={Id}] Can't cancel. Task is already cleared of canceled\n{Url}");
                return;
            }
            if (LogLevel.IsActive(DebugLevel.Log))
                Debug.Log($"[ImageLoader] Future[id={Id}] Cancel\n{Url}");
            Status = FutureStatus.Canceled;
            if (Safe.RunCancel(cts, LogLevel))
                Safe.Run(OnCanceled, LogLevel);
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
                if (weakReference.TryGetTarget(out var reference))
                    reference.Loaded(new Reference<T>(url, obj), FutureLoadedFrom.MemoryCache);
            });
            LoadingFromDiskCache(() =>
            {
                if (weakReference.TryGetTarget(out var reference))
                    reference.Loading(FutureLoadingFrom.DiskCache);
            });
            LoadedFromDiskCache(obj =>
            {
                if (weakReference.TryGetTarget(out var reference))
                    reference.Loaded(new Reference<T>(url, obj), FutureLoadedFrom.DiskCache);
            });
            LoadingFromSource(() =>
            {
                if (weakReference.TryGetTarget(out var reference))
                    reference.Loading(FutureLoadingFrom.Source);
            });
            LoadedFromSource(obj =>
            {
                if (weakReference.TryGetTarget(out var reference))
                    reference.Loaded(new Reference<T>(url, obj), FutureLoadedFrom.Source);
            });
            Failed(e =>
            {
                if (weakReference.TryGetTarget(out var reference))
                    reference.FailToLoad(e);
            });
            Canceled(() =>
            {
                if (weakReference.TryGetTarget(out var reference))
                    reference.Cancel();
            });

            // WARNING: It creates cross reference between two Future instances
            // Which doesn't let to dispose non of them until the other one is disposed explicitly
            // TODO: Find a way to dispose the cross reference automatically. WeakReference for one of them?
            futureRef.Canceled(Cancel);

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
            WebRequest = null;

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
            var uts = new UniTaskCompletionSource<T>();

            Then(value => uts.TrySetResult(value));
            Failed(exception => uts.TrySetException(exception));
            Canceled(() => uts.TrySetCanceled());

            return uts.Task;
        }
        public Task<T> AsTask()
        {
            var tcs = new TaskCompletionSource<T>();

            Then(tcs.SetResult);
            Failed(tcs.SetException);
            Canceled(tcs.SetCanceled);

            return tcs.Task;
        }
        public FutureAwaiter<T> GetAwaiter()
        {
            var tcs = new TaskCompletionSource<T>();

            Then(tcs.SetResult);
            Failed(tcs.SetException);
            Canceled(tcs.SetCanceled);

            return new FutureAwaiter<T>(tcs.Task.GetAwaiter());
        }
    }
}
