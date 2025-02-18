using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Extensions.Unity.ImageLoader
{
    public interface IFuture
    {
        uint Id { get; }
        string Url { get; }
        bool IsCancelled { get; }
        bool IsLoaded { get; }
        bool IsCompleted { get; }
        bool IsInProgress { get; }
        FutureStatus Status { get; }
        CancellationToken CancellationToken { get; }
        void Cancel();
        void Forget();
    }
    public partial interface IFuture<T> : IFuture, IDisposable
    {
        T Value { get; }
        DebugLevel LogLevel { get; }
        UniTask StartLoading(bool ignoreImageNotFoundError = false);
        IFuture<T> Then(Action<T> onCompleted);
        // Future<T> ThenSet<C>(Action<T, C> setter);
        IFuture<T> Failed(Action<Exception> action);
        IFuture<T> Completed(Action<bool> action);
        IFuture<T> LoadedFromMemoryCache(Action<T> action);
        IFuture<T> LoadingFromDiskCache(Action action);
        IFuture<T> LoadedFromDiskCache(Action<T> action);
        IFuture<T> LoadingFromSource(Action action);
        IFuture<T> LoadedFromSource(Action<T> action);
        IFuture<T> Canceled(Action action);

        IFuture<T> SetUseDiskCache(bool value = true);
        IFuture<T> SetUseMemoryCache(bool value = true);
        IFuture<T> SetLogLevel(DebugLevel value);

        IFuture<T> PassEvents(IFutureInternal<T> to, bool passCancelled = true);
        IFuture<T> PassEvents<T2>(IFutureInternal<T2> to, Func<T, T2> convert, bool passCancelled = true);

        IFuture<Reference<T>> AsReference(DebugLevel logLevel = DebugLevel.Trace);
        UniTask<T> AsUniTask();
        Task<T> AsTask();

        FutureAwaiter<T> GetAwaiter();
    }

    public interface IFutureInternal<T> : IFuture<T>
    {
        void FailToLoad(Exception exception);
        void Loading(FutureLoadingFrom loadingFrom);
        void Loaded(T value, FutureLoadedFrom loadedFrom);
    }
}