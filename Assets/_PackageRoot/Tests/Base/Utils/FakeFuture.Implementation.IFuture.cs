using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    internal static class FakeFutureMetadata
    {
        public static volatile uint idCounter = 0;
    }
    public partial class FakeFuture<T> : IFuture<T>
    {
        public T Value { get; private set; }

        public DebugLevel LogLevel { get; private set; }

        public uint Id { get; } = FakeFutureMetadata.idCounter++;

        public string Url { get; }

        public bool IsCancelled => throw new NotImplementedException();

        public bool IsLoaded => throw new NotImplementedException();

        public bool IsCompleted => throw new NotImplementedException();

        public bool IsInProgress => throw new NotImplementedException();

        public FutureStatus Status => throw new NotImplementedException();

        public CancellationToken CancellationToken => throw new NotImplementedException();

        public IFuture<Reference<T>> AsReference(DebugLevel logLevel = DebugLevel.Trace) => throw new NotImplementedException();
        public IEnumerator AsCoroutine(Action<T> resultHandler = null, Action<Exception> exceptionHandler = null) => throw new NotImplementedException();
        public Task<T> AsTask() => throw new NotImplementedException();
        public UniTask<T> AsUniTask() => throw new NotImplementedException();
        public void Cancel()
        {
            lock (events)
                events.Add(new EventData { name = EventName.Canceled });
        }
        public IFuture<T> Canceled(Action action) => throw new NotImplementedException();
        public IFuture<T> Completed(Action<bool> action) => throw new NotImplementedException();
        public IFuture<T> Failed(Action<Exception> action) => throw new NotImplementedException();
        public void Forget() => throw new NotImplementedException();
        public FutureAwaiter<T> GetAwaiter() => throw new NotImplementedException();
        public IFuture<T> LoadedFromDiskCache(Action<T> action) => throw new NotImplementedException();
        public IFuture<T> LoadedFromMemoryCache(Action<T> action) => throw new NotImplementedException();
        public IFuture<T> LoadedFromSource(Action<T> action) => throw new NotImplementedException();
        public IFuture<T> LoadingFromDiskCache(Action action, bool ignoreLoaded = false) => throw new NotImplementedException();
        public IFuture<T> LoadingFromSource(Action action, bool ignoreLoaded = false) => throw new NotImplementedException();
        public IFuture<T> Consume(Action<T> setter, bool replace = false) => throw new NotImplementedException();
        public IFuture<T> PassEvents(IFutureInternal<T> to, bool passCancelled = true) => throw new NotImplementedException();
        public IFuture<T> PassEvents<T2>(IFutureInternal<T2> to, Func<T, T2> convert, bool passCancelled = true) => throw new NotImplementedException();
        public IFuture<T> SetLogLevel(DebugLevel value) => throw new NotImplementedException();
        public IFuture<T> SetUseDiskCache(bool value = true) => throw new NotImplementedException();
        public IFuture<T> SetUseMemoryCache(bool value = true) => throw new NotImplementedException();
        public IFuture<T> SetPlaceholder(T placeholder, params PlaceholderTrigger[] triggers) => throw new NotImplementedException();
        public UniTask StartLoading(bool ignoreImageNotFoundError = false) => throw new NotImplementedException();
        public IFuture<T> Loaded(Action<T> onCompleted) => throw new NotImplementedException();
    }
}