using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    public static partial class TestUtils
    {
        // Starts loading a sprite while guaranteeing that, when usePlaceholder is set,
        // the loading-state placeholders are registered BEFORE the load can advance past
        // the loading state.
        //
        // ImageLoader.LoadSprite(url) starts the load synchronously inside the call, so a
        // fast (disk/memory-cache) load can reach LoadedFrom* before the test gets a
        // chance to call SetPlaceholder afterwards. When that happens the loading-state
        // placeholder is never consumed and the expected first "Consumed" event is
        // missing — a timing flake (seen on fast CI runners). Creating the future
        // un-started, attaching the listener, registering the placeholders, and only then
        // calling StartLoading() makes the loading-placeholder consume deterministic.
        //
        // cancelAtLoadingFrom makes "cancel-while-loading-from-disk-cache" deterministic.
        // The disk read is a local File.ReadAllBytes dispatched to a TaskFactory; on a
        // fast machine that Task can complete (and the awaiting continuation resume
        // inline) entirely inside StartLoading(), so the future reaches
        // LoadedFromDiskCache before the test's post-StartLoading Cancel() runs — the
        // test then sees a successful load instead of the cancel (observed once on a
        // windows-mono standalone leg). To cancel reliably *while in the loading state*
        // we hook the cancel to the last synchronous loading-state callback so the
        // event order is preserved exactly:
        //   - usePlaceholder == false: cancel from the LoadingFrom... event (no consume
        //     happens), yielding LoadingFrom..., Canceled, Completed.
        //   - usePlaceholder == true: cancel from the loading-placeholder Consume, which
        //     runs after the listener has recorded the loading event and its Consume,
        //     yielding LoadingFrom..., Consumed, Canceled, Consumed, Completed.
        // The runtime re-checks IsCancelled after the disk read (Future.Loading.cs), so a
        // cancel raised here reliably wins over the read result.
        static FutureListener<UnityEngine.Sprite> StartLoadSpriteWithPlaceholders(
            string url, bool usePlaceholder, bool ignoreLoadingWhenLoaded, out FutureSprite future,
            FutureLoadingFrom? cancelAtLoadingFrom = null)
        {
            future = new FutureSprite(url);
            var listener = future.ToFutureListener(ignoreLoadingWhenLoaded: ignoreLoadingWhenLoaded, ignorePlaceholder: !usePlaceholder);

            if (usePlaceholder)
            {
                future.SetPlaceholder(placeholderSprites[PlaceholderTrigger.LoadingFromDiskCache], PlaceholderTrigger.LoadingFromDiskCache);
                future.SetPlaceholder(placeholderSprites[PlaceholderTrigger.LoadingFromSource], PlaceholderTrigger.LoadingFromSource);
                future.SetPlaceholder(placeholderSprites[PlaceholderTrigger.FailedToLoad], PlaceholderTrigger.FailedToLoad);
                future.SetPlaceholder(placeholderSprites[PlaceholderTrigger.Canceled], PlaceholderTrigger.Canceled);
            }

            if (cancelAtLoadingFrom.HasValue)
            {
                var self = future;
                var loadingPlaceholderStatus = cancelAtLoadingFrom.Value == FutureLoadingFrom.Source
                    ? FutureStatus.LoadingFromSource
                    : FutureStatus.LoadingFromDiskCache;
                var armed = true;
                if (usePlaceholder)
                {
                    // Cancel from the loading-placeholder consume (runs after the loading
                    // event + its consume have been recorded, while still in the loading
                    // state) so the consume/cancel order matches the expected sequence.
                    future.Consume(value =>
                    {
                        if (armed && self.Status == loadingPlaceholderStatus)
                        {
                            armed = false;
                            self.Cancel();
                        }
                    });
                }
                else
                {
                    void CancelOnce()
                    {
                        if (!armed) return;
                        armed = false;
                        self.Cancel();
                    }
                    if (cancelAtLoadingFrom.Value == FutureLoadingFrom.Source)
                        future.LoadingFromSource(CancelOnce);
                    else
                        future.LoadingFromDiskCache(CancelOnce);
                }
            }

            future.StartLoading();
            return listener;
        }

        public static IEnumerator LoadFromMemoryCache(string url, bool usePlaceholder = false) => Load(url, null, FutureLoadedFrom.MemoryCache, usePlaceholder);
        public static IEnumerator Load(string url, FutureLoadingFrom? expectedLoadingFrom, FutureLoadedFrom expectedLoadedFrom, bool usePlaceholder = false)
        {
            var futureListener = StartLoadSpriteWithPlaceholders(url, usePlaceholder, ignoreLoadingWhenLoaded: false, out var future);

            if (expectedLoadingFrom.HasValue)
                futureListener.Assert_Events_Contains(expectedLoadingFrom.Value.ToEventName());

            var task1 = future.AsTask();
            yield return future.TimeoutCoroutine(TimeSpan.FromSeconds(10));
            var task2 = future.AsTask();

            futureListener.Assert_Events_NotContains(EventName.Canceled);

            var events = expectedLoadingFrom.HasValue
                ? usePlaceholder
                    ? new [] { expectedLoadingFrom.Value.ToEventName(), EventName.Consumed, expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Consumed, EventName.Completed }
                    : new [] { expectedLoadingFrom.Value.ToEventName(), expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Completed }
                : usePlaceholder
                    ? new [] { expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Consumed, EventName.Completed }
                    : new [] { expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Completed };

            futureListener.Assert_Events_Equals(events);
            futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            Assert.IsTrue(task1.IsCompleted, "Task was not cancelled but Future was cancelled");
            Assert.IsTrue(task2.IsCompleted, "Task was not cancelled but Future was cancelled");

            yield return UniTask.Yield();

            futureListener.Assert_Events_Equals(events);
            futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            Assert.AreEqual(future.Status, expectedLoadedFrom.AsFutureStatus());

            future.ToFutureListener(ignoreLoadingWhenLoaded: true, ignorePlaceholder: true)
                .Assert_Events_Equals(expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Completed)
                .Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            future.ToFutureListener(ignoreLoadingWhenLoaded: true, ignorePlaceholder: false)
                .Assert_Events_Equals(expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Consumed, EventName.Completed)
                .Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            if (expectedLoadingFrom.HasValue)
                future.ToFutureListener()
                    .Assert_Events_Equals(expectedLoadingFrom.Value.ToEventName(), expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Completed)
                    .Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            future.Dispose();
            yield return UniTask.Yield();
        }
        public static IEnumerator LoadFromMemoryCacheThenCancel(string url, bool useGC, bool usePlaceholder = false)
            => LoadThenCancel(url, null, FutureLoadedFrom.MemoryCache, useGC, usePlaceholder);
        public static IEnumerator LoadThenCancel(string url, FutureLoadingFrom? expectedLoadingFrom, FutureLoadedFrom expectedLoadedFrom, bool useGC, bool usePlaceholder = false)
        {
            var futureListener = StartLoadSpriteWithPlaceholders(url, usePlaceholder, ignoreLoadingWhenLoaded: false, out var future);

            if (expectedLoadingFrom.HasValue)
                futureListener.Assert_Events_Contains(expectedLoadingFrom.Value.ToEventName());

            var task1 = future.AsTask();
            yield return future.TimeoutCoroutine(TimeSpan.FromSeconds(10));
            var task2 = future.AsTask();

            futureListener.Assert_Events_NotContains(EventName.Canceled);

            if (useGC)
                TestUtils.WaitForGCFast();

            future.Cancel();

            var events = expectedLoadingFrom.HasValue
                ? usePlaceholder
                    ? new [] { expectedLoadingFrom.Value.ToEventName(), EventName.Consumed, expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Consumed, EventName.Completed }
                    : new [] { expectedLoadingFrom.Value.ToEventName(), expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Completed }
                : usePlaceholder
                    ? new [] { expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Consumed, EventName.Completed }
                    : new [] { expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Completed };

            futureListener.Assert_Events_Equals(events);
            futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            Assert.IsTrue(task1.IsCompleted, "Task was not cancelled but Future was cancelled. Probably the OnCancel subscription was cleaned up too early.");
            Assert.IsTrue(task2.IsCompleted, "Task was not cancelled but Future was cancelled. Probably the OnCancel subscription was cleaned up too early.");

            yield return UniTask.Yield();

            futureListener.Assert_Events_Equals(events);
            futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            Assert.AreEqual(future.Status, expectedLoadedFrom.AsFutureStatus());

            future.ToFutureListener(ignoreLoadingWhenLoaded: true, ignorePlaceholder: true)
                .Assert_Events_Equals(expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Completed)
                .Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            future.ToFutureListener(ignoreLoadingWhenLoaded: true, ignorePlaceholder: false)
                .Assert_Events_Equals(expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Consumed, EventName.Completed)
                .Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            if (expectedLoadingFrom.HasValue)
                future.ToFutureListener()
                    .Assert_Events_Equals(expectedLoadingFrom.Value.ToEventName(), expectedLoadedFrom.ToEventName(), EventName.Loaded, EventName.Completed)
                    .Assert_Events_Value<bool>(EventName.Completed, success => success == true);

            future.Dispose();
            yield return UniTask.Yield();
        }
        public static IEnumerator LoadFromMemoryCacheAndCancel(string url, bool usePlaceholder = false) => LoadAndCancel(url, null, usePlaceholder);
        public static IEnumerator LoadAndCancel(string url, FutureLoadingFrom? expectedLoadingFrom, bool usePlaceholder = false)
        {
            yield return LoadAndCancel(url, expectedLoadingFrom, useGC: true, usePlaceholder);
            yield return LoadAndCancel(url, expectedLoadingFrom, useGC: false, usePlaceholder);
        }
        public static IEnumerator LoadAndCancel(string url, FutureLoadingFrom? expectedLoadingFrom, bool useGC, bool usePlaceholder = false)
        {
            // For a load from disk cache the read can complete synchronously inside
            // StartLoading() and beat the explicit Cancel() below, so cancel at the
            // loading transition instead (see StartLoadSpriteWithPlaceholders). The
            // explicit Cancel() further down then becomes a harmless no-op. Source loads
            // go through UnityWebRequest (never synchronous) and memory-cache loads are
            // not a cancel-while-loading scenario, so neither needs this.
            var cancelAtLoadingFrom = expectedLoadingFrom.HasValue && expectedLoadingFrom.Value == FutureLoadingFrom.DiskCache
                ? expectedLoadingFrom
                : null;
            var futureListener = StartLoadSpriteWithPlaceholders(url, usePlaceholder, ignoreLoadingWhenLoaded: false, out var future, cancelAtLoadingFrom);
            var shouldLoadFromMemoryCache = !expectedLoadingFrom.HasValue;

            futureListener.Assert_Events_Contains(expectedLoadingFrom.HasValue
                ? expectedLoadingFrom.Value.ToEventName()
                : EventName.LoadedFromMemoryCache);

            if (useGC)
                TestUtils.WaitForGCFast();

            var task1 = future.AsTask();
            future.Cancel();
            var task2 = future.AsTask();

            var events = shouldLoadFromMemoryCache
                ? usePlaceholder
                    ? new [] { EventName.LoadedFromMemoryCache, EventName.Loaded, EventName.Consumed, EventName.Completed }
                    : new [] { EventName.LoadedFromMemoryCache, EventName.Loaded, EventName.Completed }
                : usePlaceholder
                    ? new [] { expectedLoadingFrom.Value.ToEventName(), EventName.Consumed, EventName.Canceled, EventName.Consumed, EventName.Completed }
                    : new [] { expectedLoadingFrom.Value.ToEventName(), EventName.Canceled, EventName.Completed };

            futureListener.Assert_Events_Equals(events);
            futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == shouldLoadFromMemoryCache);

            Assert.IsTrue(task1.IsCompleted, "Task was not cancelled but Future was cancelled. Probably the OnCancel subscription was cleaned up too early.");
            Assert.IsTrue(task2.IsCompleted, "Task was not cancelled but Future was cancelled. Probably the OnCancel subscription was cleaned up too early.");

            yield return UniTask.Yield();

            futureListener.Assert_Events_Equals(events);
            futureListener.Assert_Events_Value<bool>(EventName.Completed, success => success == shouldLoadFromMemoryCache);

            Assert.AreEqual(future.Status, shouldLoadFromMemoryCache
                ? FutureStatus.LoadedFromMemoryCache
                : FutureStatus.Canceled);

            var lateEvents = shouldLoadFromMemoryCache
                ? usePlaceholder
                    ? new[] { EventName.LoadedFromMemoryCache, EventName.Loaded, EventName.Consumed, EventName.Completed }
                    : new[] { EventName.LoadedFromMemoryCache, EventName.Loaded, EventName.Completed }
                : usePlaceholder
                    ? new[] { expectedLoadingFrom.Value.ToEventName(), EventName.Canceled, EventName.Consumed, EventName.Completed }
                    : new[] { expectedLoadingFrom.Value.ToEventName(), EventName.Canceled, EventName.Completed };

            var lateFutureListener = future.ToFutureListener(ignorePlaceholder: !usePlaceholder)
                .Assert_Events_Equals(lateEvents)
                .Assert_Events_Value<bool>(EventName.Completed, success => success == shouldLoadFromMemoryCache);

            if (expectedLoadingFrom.HasValue && future.IsLoaded)
                future.ToFutureListener(ignoreLoadingWhenLoaded: true, ignorePlaceholder: !usePlaceholder)
                    .Assert_Events_Equals(lateEvents.Except(new [] { expectedLoadingFrom.Value.ToEventName() }))
                    .Assert_Events_Value<bool>(EventName.Completed, success => success == shouldLoadFromMemoryCache);

            future.Dispose();
            yield return UniTask.Yield();
        }
    }
}