using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T>
    {
        /// <summary>
        ///
        /// </summary>
        public IFuture<T> SetPlaceholder(T placeholder, params FuturePlaceholderTrigger[] triggers)
        {
            if (cleared || IsCancelled)
            {
                if (LogLevel.IsActive(DebugLevel.Error))
                    Debug.Log($"[ImageLoader] Future[id={Id}] SetPlaceholder: is impossible because the future is cleared or canceled\n{Url}");
                return this;
            }

            foreach (var trigger in triggers)
            {
                if (trigger.IsEqual(Status))
                {
                    lock (setters)
                    {
                        foreach (var setter in setters)
                            Safe.Run(setter, placeholder, LogLevel);
                    }
                    continue;
                }
                placeholders[trigger.AsFutureStatus()] = placeholder;
                // switch (trigger)
                // {
                //     case FuturePlaceholderTrigger.LoadingFromDiskCache:
                //         LoadingFromDiskCache(() =>
                //         {
                //             lock (settersAll)
                //             {
                //                 foreach (var setter in settersAll)
                //                     Safe.Run(setter, placeholder, LogLevel);
                //             }
                //         });
                //         break;
                //     case FuturePlaceholderTrigger.LoadingFromSource:
                //         LoadingFromDiskCache(() =>
                //         {
                //             lock (settersAll)
                //             {
                //                 foreach (var setter in settersAll)
                //                     Safe.Run(setter, placeholder, LogLevel);
                //             }
                //         });
                //         break;
                //     case FuturePlaceholderTrigger.FailedToLoad:
                //         Failed(exception =>
                //         {
                //             lock (settersAll)
                //             {
                //                 foreach (var setter in settersAll)
                //                     Safe.Run(setter, placeholder, LogLevel);
                //             }
                //         });
                //         break;
                //     case FuturePlaceholderTrigger.Canceled:
                //         Canceled(() =>
                //         {
                //             lock (settersAll)
                //             {
                //                 foreach (var setter in settersAll)
                //                     Safe.Run(setter, placeholder, LogLevel);
                //             }
                //         });
                //         break;
                //}
            }

            return this;
        }
    }
}
