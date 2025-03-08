using System;
using System.Collections.Concurrent;
using System.Linq;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public static partial class FutureEx
    {
        const int PlaceholderSolidColorTextureSize = 2;
        static ConcurrentDictionary<Type, ConcurrentDictionary<string, object>> colors = new ConcurrentDictionary<Type, ConcurrentDictionary<string, object>>();

        public static Texture2D CreateFillTexture(this Color color, int width = PlaceholderSolidColorTextureSize, int height = PlaceholderSolidColorTextureSize)
        {
            var texture = new Texture2D(width, height);
            texture.SetPixels(Enumerable.Repeat(color, width * height).ToArray());
            texture.Apply();
            return texture;
        }

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
