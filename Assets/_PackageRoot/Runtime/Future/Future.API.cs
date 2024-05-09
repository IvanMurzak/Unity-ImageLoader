using Cysharp.Threading.Tasks;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T> : IDisposable
    {
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
        public Future<T> Cancelled(Action action)
        {
            if (IsCancelled)
            {
                action();
                return this;
            }
            OnCancelled += action;
            return this;
        }

        public void Cancel()
        {
            if (cleared || IsCancelled) return;
            if (ImageLoader.settings.debugLevel <= DebugLevel.Log)
                Debug.Log($"[ImageLoader] Cancel: {Url}");
            Status = FutureStatus.Canceled;
            cts.Cancel();
            OnCancelled?.Invoke();
            Clear();
        }
        public void Dispose()
        {
            Clear();
            Status = FutureStatus.Disposed;
            if (value is IDisposable disposable)
                disposable?.Dispose();
            value = default;
            exception = default;
        }
        public void Forget()
        {
            var awaiter = GetAwaiter();
            if (awaiter.IsCompleted)
            {
                try
                {
                    awaiter.GetResult();
                }
                catch (Exception ex)
                {
                    if (ImageLoader.settings.debugLevel <= DebugLevel.Exception)
                        Debug.LogException(ex);
                }
            }
            else
            {
                awaiter.OnCompleted(() =>
                {
                    try
                    {
                        awaiter.GetResult();
                    }
                    catch (Exception ex)
                    {
                        if (ImageLoader.settings.debugLevel <= DebugLevel.Exception)
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
            Cancelled(tcs.SetCanceled);
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
