using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T>
    {
        private static ConcurrentDictionary<string, Future<T>> loadingInProcess = new ConcurrentDictionary<string, Future<T>>();
        private static bool RegisterLoading(Future<T> future, out Future<T> anotherLoadingFuture)
        {
            lock (loadingInProcess)
            {
                if (loadingInProcess.TryGetValue(future.Url, out anotherLoadingFuture))
                    return false;

                if (!loadingInProcess.TryAdd(future.Url, future))
                    throw new Exception($"[ImageLoader] Future[id={future.id}] Can't start new loading. Because loading is in progress\n{future.Url}");

                if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Log))
                    Debug.Log($"[ImageLoader] Future[id={future.id}] Loading. Total {loadingInProcess.Count} loading tasks\n{future.Url}");

                return true;
            }
        }
        private static void RemoveLoading(Future<T> future) => RemoveLoading(future.Url);
        private static void RemoveLoading(string url)
        {
            lock (loadingInProcess)
            {
                if (loadingInProcess.TryRemove(url, out var future))
                {
                    if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Log))
                        Debug.Log($"[ImageLoader] Future[id={future.id}] RemoveLoading: left {loadingInProcess.Count} loading tasks\n{url}");
                }
                else
                {
                    if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Warning))
                        Debug.LogWarning($"[ImageLoader] Future[id={future.id}] RemoveLoading: not found in loading tasks\n{url}");
                }
            }
        }

        /// <summary>
        /// Check if the url is loading right now
        /// </summary>
        /// <returns>Returns true if the url is loading right now</returns>
        public static bool IsLoading(string url) => loadingInProcess.ContainsKey(url);

        /// <summary>
        /// Find and return current loading Future by the url
        /// <param name="url">URL to the picture, web or local</param>
        /// </summary>
        /// <returns>Returns current loading Future or null if none</returns>
        public static Future<T> GetLoadingFuture(string url)
            => loadingInProcess.TryGetValue(url, out var future)
                ? future
                : null;

        /// <summary>
        /// Return all current loading Futures
        /// </summary>
        /// <returns>Returns read only list of all current loading Futures</returns>
        public static IReadOnlyCollection<Future<T>> GetLoadingFutures() => loadingInProcess.Values.ToArray();
    }
}
