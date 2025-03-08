using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public static partial class FutureEx
    {
        /// <summary>
        /// Set a placeholder in all conditions for this Future instance
        /// </summary>
        /// <param name="color">new placeholder</param>
        /// <returns>Returns the Future instance</returns>
        public static IFuture<Texture2D> SetPlaceholder(this IFuture<Texture2D> future, Color color) => future.SetPlaceholder(color,
            PlaceholderTrigger.LoadingFromDiskCache,
            PlaceholderTrigger.LoadingFromSource,
            PlaceholderTrigger.FailedToLoad,
            PlaceholderTrigger.Canceled);

        /// <summary>
        /// Set a placeholder in specified conditions for this Future instance
        /// </summary>
        /// <param name="color">hex color to fill solid color</param>
        /// <returns>Returns the Future instance</returns>
        public static IFuture<Texture2D> SetPlaceholder(this IFuture<Texture2D> future, Color color, params PlaceholderTrigger[] triggers)
            => future.SetPlaceholder(ColorUtility.ToHtmlStringRGBA(color), triggers);

        /// <summary>
        /// Set a placeholder in all conditions for this Future instance
        /// </summary>
        /// <param name="color">new placeholder</param>
        /// <returns>Returns the Future instance</returns>
        public static IFuture<Sprite> SetPlaceholder(this IFuture<Sprite> future, Color color) => future.SetPlaceholder(color,
            PlaceholderTrigger.LoadingFromDiskCache,
            PlaceholderTrigger.LoadingFromSource,
            PlaceholderTrigger.FailedToLoad,
            PlaceholderTrigger.Canceled);

        /// <summary>
        /// Set a placeholder in specified conditions for this Future instance
        /// </summary>
        /// <param name="hexColor">hex color to fill solid color</param>
        /// <returns>Returns the Future instance</returns>
        public static IFuture<Sprite> SetPlaceholder(this IFuture<Sprite> future, Color color, params PlaceholderTrigger[] triggers)
            => future.SetPlaceholder(ColorUtility.ToHtmlStringRGBA(color), triggers);

        /// <summary>
        /// Set a placeholder in all conditions for this Future instance
        /// </summary>
        /// <param name="color">new placeholder</param>
        /// <returns>Returns the Future instance</returns>
        public static IFuture<Reference<Texture2D>> SetPlaceholder(this IFuture<Reference<Texture2D>> future, Color color) => future.SetPlaceholder(color,
            PlaceholderTrigger.LoadingFromDiskCache,
            PlaceholderTrigger.LoadingFromSource,
            PlaceholderTrigger.FailedToLoad,
            PlaceholderTrigger.Canceled);

        /// <summary>
        /// Set a placeholder in specified conditions for this Future instance
        /// </summary>
        /// <param name="hexColor">hex color to fill solid color</param>
        /// <returns>Returns the Future instance</returns>
        public static IFuture<Reference<Texture2D>> SetPlaceholder(this IFuture<Reference<Texture2D>> future, Color color, params PlaceholderTrigger[] triggers)
            => future.SetPlaceholder(ColorUtility.ToHtmlStringRGBA(color), triggers);

        /// <summary>
        /// Set a placeholder in all conditions for this Future instance
        /// </summary>
        /// <param name="color">new placeholder</param>
        /// <returns>Returns the Future instance</returns>
        public static IFuture<Reference<Sprite>> SetPlaceholder(this IFuture<Reference<Sprite>> future, Color color) => future.SetPlaceholder(color,
            PlaceholderTrigger.LoadingFromDiskCache,
            PlaceholderTrigger.LoadingFromSource,
            PlaceholderTrigger.FailedToLoad,
            PlaceholderTrigger.Canceled);

        /// <summary>
        /// Set a placeholder in specified conditions for this Future instance
        /// </summary>
        /// <param name="hexColor">hex color to fill solid color</param>
        /// <returns>Returns the Future instance</returns>
        public static IFuture<Reference<Sprite>> SetPlaceholder(this IFuture<Reference<Sprite>> future, Color color, params PlaceholderTrigger[] triggers)
            => future.SetPlaceholder(ColorUtility.ToHtmlStringRGBA(color), triggers);
    }
}
