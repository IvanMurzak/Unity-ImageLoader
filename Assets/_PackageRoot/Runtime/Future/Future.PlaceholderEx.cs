using UnityEditor.PackageManager;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public static partial class FutureEx
    {
        /// <summary>
        /// Set a placeholder in all conditions for this Future instance
        /// </summary>
        /// <param name="placeholder">new placeholder</param>
        /// <returns>Returns the Future instance</returns>
        public static IFuture<T> SetPlaceholder<T>(this IFuture<T> future, T placeholder) => future.SetPlaceholder(placeholder,
            PlaceholderTrigger.LoadingFromDiskCache,
            PlaceholderTrigger.LoadingFromSource,
            PlaceholderTrigger.FailedToLoad,
            PlaceholderTrigger.Canceled);
    }
}
