using System;
using UnityEngine.Networking;

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    public partial class FakeFuture<T> : IFutureInternal<T>
    {
        public UnityWebRequest WebRequest => throw new NotImplementedException();

        public void Loading(FutureLoadingFrom loadingFrom)
        {
            if (LogLevel.IsActive(DebugLevel.Trace))
                UnityEngine.Debug.Log($"FakeFuture[id={Id}] Loading from {loadingFrom}");

            var eventName = loadingFrom.ToEventName();
            lock (events)
                events.Add(new EventData { name = eventName });
        }

        public void SetLoaded(T value, FutureLoadedFrom loadedFrom)
        {
            if (LogLevel.IsActive(DebugLevel.Trace))
                UnityEngine.Debug.Log($"FakeFuture[id={Id}] Loaded from {loadedFrom}");

            var eventName = loadedFrom.ToEventName();
            lock (events)
                events.Add(new EventData { name = eventName, value = value });
        }

        public void FailToLoad(Exception exception)
        {
            if (LogLevel.IsActive(DebugLevel.Trace))
                UnityEngine.Debug.Log($"FakeFuture[id={Id}] FailToLoad: {exception}");
            lock (events)
                events.Add(new EventData { name = EventName.Failed, value = exception });
        }

        public void SetTimeout(TimeSpan duration) => throw new NotImplementedException();
    }
}