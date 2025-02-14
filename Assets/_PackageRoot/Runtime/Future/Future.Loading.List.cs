using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T>
    {
        private static ConcurrentDictionary<string, Future<T>> loadingInProcess = new ConcurrentDictionary<string, Future<T>>();
        private static void AddLoading(Future<T> future)
        {
            if (!loadingInProcess.TryAdd(future.Url, future))
                throw new Exception($"[ImageLoader] Future[id={future.id}] AddLoading: {future.Url} already loading");

            if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Log))
                Debug.Log($"[ImageLoader] Future[id={future.id}] AddLoading: {future.Url}, total {loadingInProcess.Count} loading tasks");
        }
        private static void RemoveLoading(Future<T> future) => RemoveLoading(future.Url);
        private static void RemoveLoading(string url)
        {
            if (loadingInProcess.TryRemove(url, out var future))
            {
                if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Log))
                    Debug.Log($"[ImageLoader] Future[id={future.id}] RemoveLoading: {url}, left {loadingInProcess.Count} loading tasks");
            }
            else
            {
                if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Warning))
                    Debug.LogWarning($"[ImageLoader] Future[id={future.id}] RemoveLoading: {url} not found in loading tasks");
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
        public static Future<T> GetLoadingFuture(string url) => loadingInProcess.TryGetValue(url, out var future) ? future : null;

        /// <summary>
        /// Return all current loading Futures
        /// </summary>
        /// <returns>Returns read only list of all current loading Futures</returns>
        public static IReadOnlyCollection<Future<T>> GetLoadingFutures() => loadingInProcess.Values.ToArray();
    }
}
