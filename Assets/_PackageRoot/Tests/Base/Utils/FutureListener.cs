using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    public partial class FutureListener<T>
    {
        List<EventData> events = new List<EventData>();

        public IReadOnlyList<EventData> Events
        {
            get
            {
                lock (events)
                    return events.Select(x => x).ToList().AsReadOnly();
            }
        }

        public FutureListener(IFuture<T> future, bool ignoreLoadingWhenLoaded = false, bool ignorePlaceholder = false, DebugLevel? logLevel = null)
        {
            if (logLevel == null)
                logLevel = ImageLoader.settings.debugLevel;

            if (logLevel.Value.IsActive(DebugLevel.Trace))
                Debug.Log($"[FutureListener] Future[id={future.Id}] Created");
            future.LoadedFromMemoryCache(value =>
            {
                if (logLevel.Value.IsActive(DebugLevel.Trace))
                    Debug.Log($"[FutureListener] Future[id={future.Id}] LoadedFromMemoryCache: {value}");
                lock (events)
                    events.Add(new EventData { name = EventName.LoadedFromMemoryCache, value = value });
            });
            future.LoadingFromDiskCache(() =>
            {
                if (logLevel.Value.IsActive(DebugLevel.Trace))
                    Debug.Log($"[FutureListener] Future[id={future.Id}] LoadingFromDiskCache");
                lock (events)
                    events.Add(new EventData { name = EventName.LoadingFromDiskCache });
            }, ignoreWhenLoaded: ignoreLoadingWhenLoaded);
            future.LoadedFromDiskCache(value =>
            {
                if (logLevel.Value.IsActive(DebugLevel.Trace))
                    Debug.Log($"[FutureListener] Future[id={future.Id}] LoadedFromDiskCache: {value}");
                lock (events)
                    events.Add(new EventData { name = EventName.LoadedFromDiskCache, value = value });
            });
            future.LoadingFromSource(() =>
            {
                if (logLevel.Value.IsActive(DebugLevel.Trace))
                    Debug.Log($"[FutureListener] Future[id={future.Id}] LoadingFromSource");
                lock (events)
                    events.Add(new EventData { name = EventName.LoadingFromSource });
            }, ignoreWhenLoaded: ignoreLoadingWhenLoaded);
            future.LoadedFromSource(value =>
            {
                if (logLevel.Value.IsActive(DebugLevel.Trace))
                    Debug.Log($"[FutureListener] Future[id={future.Id}] LoadedFromSource: {value}");
                lock (events)
                    events.Add(new EventData { name = EventName.LoadedFromSource, value = value });
            });
            future.Loaded(value =>
            {
                if (logLevel.Value.IsActive(DebugLevel.Trace))
                    Debug.Log($"[FutureListener] Future[id={future.Id}] Then: {value}");
                lock (events)
                    events.Add(new EventData { name = EventName.Loaded, value = value });
            });
            if (!ignorePlaceholder)
            {
                future.Consume(value =>
                {
                    if (logLevel.Value.IsActive(DebugLevel.Trace))
                        Debug.Log($"[FutureListener] Future[id={future.Id}] Consume: {value}");
                    lock (events)
                        events.Add(new EventData { name = EventName.Consumed, value = value });
                });
            }
            future.Failed(exception =>
            {
                if (logLevel.Value.IsActive(DebugLevel.Trace))
                    Debug.Log($"[FutureListener] Future[id={future.Id}] Failed: {exception}");
                lock (events)
                    events.Add(new EventData { name = EventName.Failed, value = exception });
            });
            if (future.Status != FutureStatus.Disposed && !future.IsCompleted)
            {
                future.Canceled(() =>
                {
                    if (logLevel.Value.IsActive(DebugLevel.Trace))
                        Debug.Log($"[FutureListener] Future[id={future.Id}] Canceled");
                    lock (events)
                        events.Add(new EventData { name = EventName.Canceled });
                });
            }
            future.Completed(value =>
            {
                if (logLevel.Value.IsActive(DebugLevel.Trace))
                    Debug.Log($"[FutureListener] Future[id={future.Id}] Completed: {value}");
                lock (events)
                    events.Add(new EventData { name = EventName.Completed, value = value });
            });
        }
    }
}