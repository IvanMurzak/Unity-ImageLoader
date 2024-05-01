using Cysharp.Threading.Tasks;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T> : IDisposable
    {
        public readonly string Url;

        private event Action<T> OnSuccess;
        private event Action<Exception> OnFail;
        private event Action OnCancelled;

        private bool cleared = false;
        private T value = default;
        private Exception exception = default;

        public bool IsCancelled { get; private set; } = false;
        public bool Successeded { get; private set; } = false;

        internal Future(string url, CancellationToken cancellationToken)
        {
            Url = url;
            cancellationToken.Register(Cancel);
        }
        ~Future() => Dispose();
        internal void CompleteSuccess(T value)
        {
            if (cleared || IsCancelled) return;
            Successeded = true;
            this.value = value;
            OnSuccess?.Invoke(value);
            Clear();
        }
        internal void CompleteFail(Exception exception)
        {
            if (cleared || IsCancelled) return;
            Successeded = false;
            this.exception = exception;
            if (ImageLoader.settings.debugLevel <= DebugLevel.Error)
                Debug.LogError(exception.Message);
            OnFail?.Invoke(exception);
            Clear();
        }

        private void Clear()
        {
            cleared = true;
            OnSuccess = null;
            OnFail = null;
            OnCancelled = null;
        }

        public Future<T> Then(Action<T> action)
        {
            if (cleared)
            {
                if (Successeded)
                    action(value);
                return this;
            }
            OnSuccess += action;
            return this;
        }
        public Future<T> Failed(Action<Exception> action)
        {
            if (cleared)
            {
                if (!Successeded && !IsCancelled)
                    action(exception);
                return this;
            }
            OnFail += action;
            return this;
        }
        public Future<T> Cancelled(Action action)
        {
            if (cleared)
            {
                if (IsCancelled)
                    action();
                return this;
            }
            OnCancelled += action;
            return this;
        }
        public void Cancel()
        {
            if (cleared) return;
            if (IsCancelled) return;
            if (ImageLoader.settings.debugLevel <= DebugLevel.Log)
                Debug.Log($"[ImageLoader] Cancel: {Url}");
            IsCancelled = true;
            OnCancelled?.Invoke();
            Clear();
        }
        public void Dispose()
        {
            Clear();
            IsCancelled = true;
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
