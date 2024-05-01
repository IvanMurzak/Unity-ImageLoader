using Cysharp.Threading.Tasks;
using System;
using System.Runtime.CompilerServices;
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

        public bool IsCancelled { get; private set; } = false;

        internal Future(string url)
        {
            Url = url;
        }
        ~Future() => Dispose();
        internal void CompleteSuccess(T sprite)
        {
            OnSuccess?.Invoke(sprite);
            Dispose();
        }
        internal void CompleteFail(Exception exception)
        {
            if (ImageLoader.settings.debugLevel <= DebugLevel.Error)
                Debug.LogError(exception.Message);
            OnFail?.Invoke(exception);
            Dispose();
        }

        public Future<T> Then(Action<T> action)
        {
            OnSuccess += action;
            return this;
        }
        public Future<T> Fail(Action<Exception> action)
        {
            OnFail += action;
            return this;
        }
        public Future<T> Cancelled(Action action)
        {
            OnCancelled += action;
            return this;
        }
        public void Cancel()
        {
            IsCancelled = true;
            OnCancelled?.Invoke();
            Dispose();
        }
        public void Dispose()
        {
            OnSuccess = null;
            OnFail = null;
            OnCancelled = null;
        }
        public async UniTask<T> AsUniTask() => await this;
        public async Task<T> AsTask() => await this;
        public FutureAwaiter GetAwaiter()
        {
            var tcs = new TaskCompletionSource<T>();
            Then(tcs.SetResult);
            Fail(tcs.SetException);
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
