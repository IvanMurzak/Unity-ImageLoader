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
        public static IFuture<Texture2D> SetPlaceholder(this IFuture<Texture2D> future, string hexColor) => future.SetPlaceholder(hexColor,
            PlaceholderTrigger.LoadingFromDiskCache,
            PlaceholderTrigger.LoadingFromSource,
            PlaceholderTrigger.FailedToLoad,
            PlaceholderTrigger.Canceled);

        /// <summary>
        /// Set a placeholder in specified conditions for this Future instance
        /// </summary>
        /// <param name="hexColor">hex color to fill solid color</param>
        /// <returns>Returns the Future instance</returns>
        public static IFuture<Texture2D> SetPlaceholder(this IFuture<Texture2D> future, string hexColor, params PlaceholderTrigger[] triggers)
            => future.SetPlaceholder(GetOrCreate(hexColor, color
                => CreateFillTexture(color)), triggers);

        /// <summary>
        /// Set a placeholder in all conditions for this Future instance
        /// </summary>
        /// <param name="color">new placeholder</param>
        /// <returns>Returns the Future instance</returns>
        public static IFuture<Sprite> SetPlaceholder(this IFuture<Sprite> future, string hexColor) => future.SetPlaceholder(hexColor,
            PlaceholderTrigger.LoadingFromDiskCache,
            PlaceholderTrigger.LoadingFromSource,
            PlaceholderTrigger.FailedToLoad,
            PlaceholderTrigger.Canceled);


        /// <summary>
        /// Set a placeholder in specified conditions for this Future instance
        /// </summary>
        /// <param name="hexColor">hex color to fill solid color</param>
        /// <returns>Returns the Future instance</returns>
        public static IFuture<Sprite> SetPlaceholder(this IFuture<Sprite> future, string hexColor, params PlaceholderTrigger[] triggers)
            => future.SetPlaceholder(GetOrCreate(hexColor, color
                => GetOrCreate(hexColor, color2
                    => CreateFillTexture(color2))
                .ToSprite()), triggers);

        /// <summary>
        /// Set a placeholder in all conditions for this Future instance
        /// </summary>
        /// <param name="color">new placeholder</param>
        /// <returns>Returns the Future instance</returns>
        public static IFuture<Reference<Texture2D>> SetPlaceholder(this IFuture<Reference<Texture2D>> future, string hexColor) => future.SetPlaceholder(hexColor,
            PlaceholderTrigger.LoadingFromDiskCache,
            PlaceholderTrigger.LoadingFromSource,
            PlaceholderTrigger.FailedToLoad,
            PlaceholderTrigger.Canceled);
        /// <summary>
        /// Set a placeholder in specified conditions for this Future instance
        /// </summary>
        /// <param name="hexColor">hex color to fill solid color</param>
        /// <returns>Returns the Future instance</returns>
        public static IFuture<Reference<Texture2D>> SetPlaceholder(this IFuture<Reference<Texture2D>> future, string hexColor, params PlaceholderTrigger[] triggers)
            => future.SetPlaceholder(
                GetOrCreate(hexColor, color => new Reference<Texture2D>(hexColor,
                    GetOrCreate(hexColor, color2
                        => CreateFillTexture(color2))))
                    .SetKeep(), triggers);

        /// <summary>
        /// Set a placeholder in all conditions for this Future instance
        /// </summary>
        /// <param name="color">new placeholder</param>
        /// <returns>Returns the Future instance</returns>
        public static IFuture<Reference<Sprite>> SetPlaceholder(this IFuture<Reference<Sprite>> future, string hexColor) => future.SetPlaceholder(hexColor,
            PlaceholderTrigger.LoadingFromDiskCache,
            PlaceholderTrigger.LoadingFromSource,
            PlaceholderTrigger.FailedToLoad,
            PlaceholderTrigger.Canceled);

        /// <summary>
        /// Set a placeholder in specified conditions for this Future instance
        /// </summary>
        /// <param name="hexColor">hex color to fill solid color</param>
        /// <returns>Returns the Future instance</returns>
        public static IFuture<Reference<Sprite>> SetPlaceholder(this IFuture<Reference<Sprite>> future, string hexColor, params PlaceholderTrigger[] triggers)
            => future.SetPlaceholder(
                GetOrCreate(hexColor, color => new Reference<Sprite>(hexColor,
                    GetOrCreate(hexColor, color2 => // sprite
                        GetOrCreate(hexColor, color3 => // texture
                            CreateFillTexture(color3))
                        .ToSprite())))
                    .SetKeep(), triggers);

        public static T GetOrCreate<T>(string hexColor, Func<Color, T> create)
        {
            var type = typeof(T);
            if (!colors.ContainsKey(type))
                colors[type] = new ConcurrentDictionary<string, object>();

            if (!colors[type].ContainsKey(hexColor))
            {
                if (ColorUtility.TryParseHtmlString(hexColor, out var color))
                {
                    colors[type][hexColor] = create(color);
                }
                else
                {
                    Debug.LogWarning($"[ImageLoader] FutureEx.SetPlaceholder: Invalid hex color '{hexColor}'");
                    colors[type][hexColor] = create(Color.magenta);
                }
            }

            return (T)colors[type][hexColor];
        }
    }
}
