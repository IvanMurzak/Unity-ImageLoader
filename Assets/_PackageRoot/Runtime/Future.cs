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
            if (ImageLoader.settings.debugLevel <= DebugLevel.Log)
                Debug.Log($"[ImageLoader] Future: {Url}");
        }
        ~Future() => Dispose();
        internal void CompleteSuccess(T sprite)
        {
            if (ImageLoader.settings.debugLevel <= DebugLevel.Log)
                Debug.Log($"[ImageLoader] Future Complete: {Url}");
            OnSuccess?.Invoke(sprite);
            Dispose();
        }
        internal void CompleteFail(Exception exception)
        {
            if (ImageLoader.settings.debugLevel <= DebugLevel.Log)
                Debug.Log($"[ImageLoader] Future Fail: {Url}");
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
            if (ImageLoader.settings.debugLevel <= DebugLevel.Log)
                Debug.Log($"[ImageLoader] Future Cancel: {Url}");
            IsCancelled = true;
            OnCancelled?.Invoke();
            Dispose();
        }
        public void Dispose()
        {
            if (ImageLoader.settings.debugLevel <= DebugLevel.Log)
                Debug.Log($"[ImageLoader] Future Dispose: {Url}");
            OnSuccess = null;
            OnFail = null;
            OnCancelled = null;
        }
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
