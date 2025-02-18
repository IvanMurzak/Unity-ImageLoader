// using System;
// using UnityEngine;

// namespace Extensions.Unity.ImageLoader.Tests.Utils
// {
//     public partial class FakeFuture<T>
//     {
//         void Subscribe()
//         {
//             OnLoadedFromMemoryCache += value =>
//             {
//                 if (LogLevel.IsActive(DebugLevel.Trace))
//                     Debug.Log($"FakeFuture[id={Id}] OnLoadedFromMemoryCache: {value}");
//                 lock (events)
//                     events.Add(new EventData { name = EventName.LoadedFromMemoryCache, value = value });
//             };
//             OnLoadingFromDiskCache += () =>
//             {
//                 if (LogLevel.IsActive(DebugLevel.Trace))
//                     Debug.Log($"FakeFuture[id={Id}] OnLoadingFromDiskCache");
//                 lock (events)
//                     events.Add(new EventData { name = EventName.LoadingFromDiskCache });
//             };
//             OnLoadedFromDiskCache += value =>
//             {
//                 if (LogLevel.IsActive(DebugLevel.Trace))
//                     Debug.Log($"FakeFuture[id={Id}] OnLoadedFromDiskCache: {value}");
//                 lock (events)
//                     events.Add(new EventData { name = EventName.LoadedFromDiskCache, value = value });
//             };
//             OnLoadingFromSource += () =>
//             {
//                 if (LogLevel.IsActive(DebugLevel.Trace))
//                     Debug.Log($"FakeFuture[id={Id}] OnLoadingFromSource");
//                 lock (events)
//                     events.Add(new EventData { name = EventName.LoadingFromSource });
//             };
//             OnLoadedFromSource += value =>
//             {
//                 if (LogLevel.IsActive(DebugLevel.Trace))
//                     Debug.Log($"FakeFuture[id={Id}] OnLoadedFromSource: {value}");
//                 lock (events)
//                     events.Add(new EventData { name = EventName.LoadedFromSource, value = value });
//             };
//             OnLoaded += value =>
//             {
//                 if (LogLevel.IsActive(DebugLevel.Trace))
//                     Debug.Log($"FakeFuture[id={Id}] OnLoaded: {value}");
//                 lock (events)
//                     events.Add(new EventData { name = EventName.OnLoaded, value = value });
//             };
//             OnFailedToLoad += value =>
//             {
//                 if (LogLevel.IsActive(DebugLevel.Trace))
//                     Debug.Log($"FakeFuture[id={Id}] OnFailedToLoad: {value}");
//                 lock (events)
//                     events.Add(new EventData { name = EventName.Failed, value = value });
//             };
//             OnCompleted += value =>
//             {
//                 if (LogLevel.IsActive(DebugLevel.Trace))
//                     Debug.Log($"FakeFuture[id={Id}] OnCompleted: {value}");
//                 lock (events)
//                     events.Add(new EventData { name = EventName.Completed, value = value });
//             };
//             Action cancelAction = () =>
//             {
//                 if (LogLevel.IsActive(DebugLevel.Trace))
//                     Debug.Log($"FakeFuture[id={Id}] OnCanceled");
//                 lock (events)
//                     events.Add(new EventData { name = EventName.Canceled });
//             };
//             lock (cancelEvents)
//                 cancelEvents.Add(cancelAction);
//             OnCanceled += cancelAction;
//         }
//     }
// }