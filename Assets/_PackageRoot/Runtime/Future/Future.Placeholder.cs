using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T>
    {
        protected Mutex placeholderMutex = new Mutex();
        protected Dictionary<FutureLoadingFrom, T> placeholders;

        /// <summary>
        ///
        /// </summary>
        public IFuture<T> SetPlaceholder(T placeholder, params FutureLoadingFrom[] from)
        {
            lock (placeholderMutex)
            {
                if (placeholders == null)
                    placeholders = new Dictionary<FutureLoadingFrom, T>();

                foreach (var f in from)
                {
                    if (placeholders.ContainsKey(f))
                    {
                        if (LogLevel.IsActive(DebugLevel.Warning))
                            Debug.Log($"[ImageLoader] Future[id={Id}] Placeholder for loading from {f} is already set. Replacing it with new value\n{Url}");
                    }
                    if (LogLevel.IsActive(DebugLevel.Trace))
                        Debug.Log($"[ImageLoader] Future[id={Id}] Set placeholder for loading from {f}\n{Url}");
                    placeholders[f] = placeholder;
                }
            }
            if (cleared || IsCancelled)
            {
                if (LogLevel.IsActive(DebugLevel.Error))
                    Debug.Log($"[ImageLoader] Future[id={Id}] SetPlaceholder: is impossible because the future is cleared or canceled\n{Url}");
                return this;
            }
            if (IsInProgress)
            {
                // TODO: set placeholder

                return this;
            }

            if (from.Any(x => x == FutureLoadingFrom.DiskCache))
            {
                LoadingFromDiskCache(() =>
                {
                    // TODO: set placeholder
                });
            }
            if (from.Any(x => x == FutureLoadingFrom.Source))
            {
                LoadingFromSource(() =>
                {
                    // TODO: set placeholder
                });
            }

            return this;
        }
    }
}
