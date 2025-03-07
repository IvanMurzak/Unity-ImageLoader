using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T>
    {
        /// <summary>
        /// Set a placeholder in all conditions for this Future instance
        /// </summary>
        /// <param name="placeholder">new placeholder</param>
        /// <returns>Returns the Future instance</returns>
        public IFuture<T> SetPlaceholder(T placeholder) => SetPlaceholder(placeholder,
            PlaceholderTrigger.LoadingFromDiskCache,
            PlaceholderTrigger.LoadingFromSource,
            PlaceholderTrigger.FailedToLoad,
            PlaceholderTrigger.Canceled);

        /// <summary>
        /// Set a placeholder in a specific condition for this Future instance
        /// </summary>
        /// <param name="placeholder">new placeholder</param>
        /// <param name="triggers">triggers for setting the placeholder</param>
        /// <returns>Returns the Future instance</returns>
        public IFuture<T> SetPlaceholder(T placeholder, params PlaceholderTrigger[] triggers)
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
                    lock (consumers)
                    {
                        foreach (var setter in consumers)
                            Safe.Run(setter, placeholder, LogLevel);
                    }
                    continue;
                }
                lock (placeholders)
                    placeholders[trigger.AsFutureStatus()] = placeholder;
            }

            return this;
        }
    }
}
