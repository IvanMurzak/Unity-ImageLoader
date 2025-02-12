﻿using System;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T>
    {
        /// <summary>
        /// Create and return empty Future<T> instance with loading status
        /// </summary>
        public Future<T> SetPlaceholder(Texture placeholder, params FutureLoadingFrom[] from)
        {
            if (cleared || IsCancelled)
            {
                if (ImageLoader.settings.debugLevel <= DebugLevel.Error && !muteLogs)
                    Debug.Log($"[ImageLoader] Future[id={id}] SetPlaceholder: {Url}, is impossible because the future is cleared or canceled");
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
