using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T> : IDisposable, IPromise<T>
    {
        public readonly string Url;

        private event Action<T> OnSuccess;
        private event Action<Exception> OnFail;
        private event Action OnCancelled;

        private CancellationToken cancellationToken;
        private bool disposed;

        public bool IsCancelled => cancellationToken.IsCancellationRequested;

        internal Future(string url) { Url = url; }
        ~Future() => Dispose();

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
        public void Dispose()
        {
            disposed = true;
            OnSuccess = null;
            OnFail = null;
            OnCancelled = null;
        }
        
        // public UniTask<T>.Awaiter GetAwaiter()
        // {
        //     var tcs = new UniTaskCompletionSource<T>();
        //     Then(r => tcs.TrySetResult(r));
        //     Fail(e => tcs.TrySetException(e));
        //     Cancelled(() => tcs.TrySetCanceled());
        //     return tcs.Task.GetAwaiter();
        // }

        public bool TrySetResult(T value)
        {
            if (disposed) return false;
            OnSuccess?.Invoke(value);
            Dispose();
            return true;
        }

        public bool TrySetException(Exception exception)
        {
            if (disposed) return false;
            if (ImageLoader.settings.debugLevel <= DebugLevel.Error)
                Debug.LogError(exception.Message);
            OnFail?.Invoke(exception);
            Dispose();
            return true;
        }

        public bool TrySetCanceled(CancellationToken cancellationToken)
        {
            if (disposed) return false;
            this.cancellationToken = cancellationToken;
            return true;
        }
    }
}
