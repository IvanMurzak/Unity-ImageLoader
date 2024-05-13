using Cysharp.Threading.Tasks;
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
        public Future<T> Then(Action<T> action)
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
        /// When the image is failed to load from any source
        /// </summary>
        /// <param name="action">action to execute on the event</param>
        /// <returns>Returns the Future instance</returns>
        public Future<T> Failed(Action<Exception> action)
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
        public Future<T> Completed(Action<bool> action)
        {
            if (IsCompleted)
            {
                action?.Invoke(IsLoaded);
                return this;
            }
            OnCompleted += action;
            return this;
        }
        public Future<T> LoadedFromMemoryCache(Action<T> action)
        {
            if (Status == FutureStatus.LoadedFromMemoryCache)
            {
                action?.Invoke(value);
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
        public Future<T> LoadingFromDiskCache(Action action)
        {
            if (Status == FutureStatus.LoadingFromDiskCache || Status == FutureStatus.LoadedFromDiskCache)
            {
                action?.Invoke();
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
        public Future<T> LoadedFromDiskCache(Action<T> action)
        {
            if (Status == FutureStatus.LoadedFromDiskCache)
            {
                action?.Invoke(value);
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
        public Future<T> LoadingFromSource(Action action)
        {
            if (Status == FutureStatus.LoadingFromSource || Status == FutureStatus.LoadedFromSource)
            {
                action?.Invoke();
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
        public Future<T> LoadedFromSource(Action<T> action)
        {
            if (Status == FutureStatus.LoadedFromSource)
            {
                action?.Invoke(value);
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
        public Future<T> Canceled(Action action)
        {
            if (IsCancelled)
            {
                action();
                return this;
            }
            OnCanceled += action;
            return this;
        }

        /// <summary>
        /// When the Future is going to be disposed
        /// </summary>
        /// <param name="action">action to execute on the event</param>
        /// <returns>Returns the Future instance</returns>
        public Future<T> Disposed(Action<Future<T>> action)
        {
            OnDispose += action;
            return this;
        }

        /// <summary>
        /// Cancel image loading process
        /// </summary>
        public void Cancel()
        {
            if (cleared || IsCancelled) return;
            if (ImageLoader.settings.debugLevel <= DebugLevel.Log && !muteLogs)
                Debug.Log($"[ImageLoader] Cancel: {Url}");
            Status = FutureStatus.Canceled;
            cts.Cancel();
            OnCanceled?.Invoke();
            Clear();
        }

        /// <summary>
        /// Dispose the Future instance and all its references and resources. It will also cancel the loading process if it is ongoing.
        /// </summary>
        public void Dispose()
        {
            Clear();
            Status = FutureStatus.Disposed;
            OnDispose?.Invoke(this);
            OnDispose = null;

            if (value is IDisposable disposable)
                disposable?.Dispose();

            value = default;
            exception = default;
            cts.Cancel();
            cts.Dispose();
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
                    if (ImageLoader.settings.debugLevel <= DebugLevel.Exception && !muteLogs)
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
                        if (ImageLoader.settings.debugLevel <= DebugLevel.Exception && !muteLogs)
                            Debug.LogException(ex);
                    }
                });
            }
        }
        public async UniTask<T> AsUniTask() => await this;
        public async Task<T> AsTask() => await this;
        public FutureAwaiter GetAwaiter()
        {
            var tcs = new TaskCompletionSource<T>();
            Then(tcs.SetResult);
            Failed(tcs.SetException);
            Canceled(tcs.SetCanceled);
            return new FutureAwaiter(tcs.Task.GetAwaiter());
        }
        public class FutureAwaiter : INotifyCompletion
        {
            private readonly TaskAwaiter<T> _awaiter;
            public FutureAwaiter(TaskAwaiter<T> awaiter) { _awaiter = awaiter; }
            public bool IsCompleted => _awaiter.IsCompleted;
            public T GetResult() => _awaiter.GetResult();
            public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);
        }
    }
}
