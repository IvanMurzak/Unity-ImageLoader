using UnityEngine;
using System.Threading;

namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
        /// <summary>
        /// Load image from web or local path and return it as Sprite
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="textureFormat">TextureFormat for the Texture2D creation</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the image was not found by specified url</param>
        /// <returns>Returns sprite asynchronously </returns>
        public static Future<Reference<Sprite>> LoadSpriteRef(string url, TextureFormat textureFormat = TextureFormat.ARGB32, bool ignoreImageNotFoundError = false, CancellationToken cancellationToken = default)
            => LoadSpriteRef(url, new Vector2(0.5f, 0.5f), textureFormat, ignoreImageNotFoundError, cancellationToken);

        /// <summary>
        /// Load image from web or local path and return it as Sprite
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="pivot">Pivot of created Sprite</param>
        /// <param name="textureFormat">TextureFormat for the Texture2D creation</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the image was not found by specified url</param>
        /// <returns>Returns sprite asynchronously </returns>
        public static Future<Reference<Sprite>> LoadSpriteRef(string url, Vector2 pivot, TextureFormat textureFormat = TextureFormat.ARGB32, bool ignoreImageNotFoundError = false, CancellationToken cancellationToken = default)
        {
            var future = new Future<Sprite>(url, cancellationToken);
            var futureRef = new Future<Reference<Sprite>>(url, cancellationToken);

            future.LoadedFromMemoryCache(sprite => futureRef.Loaded(new Reference<Sprite>(future.Url, sprite), FutureLoadedFrom.MemoryCache));
            future.LoadingFromDiskCache (() =>     futureRef.Loading(FutureLoadingFrom.DiskCache));
            future.LoadedFromDiskCache  (sprite => futureRef.Loaded(new Reference<Sprite>(future.Url, sprite), FutureLoadedFrom.DiskCache));
            future.LoadingFromSource    (() =>     futureRef.Loading(FutureLoadingFrom.Source));
            future.LoadedFromSource     (sprite => futureRef.Loaded(new Reference<Sprite>(future.Url, sprite), FutureLoadedFrom.Source));
            future.Failed               (futureRef.FailToLoad);
            futureRef.Cancelled         (future.Cancel);

            InternalLoadSprite(future, pivot, textureFormat, ignoreImageNotFoundError);
            return futureRef;
        }

        /// <summary>
        /// Clear all references to all loaded sprites
        /// </summary>
        /// <returns>Returns sprite asynchronously </returns>
        public static void ClearRef() => Reference<Sprite>.Clear();

        /// <summary>
        /// Clear all references to a single loaded sprites by URL
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns sprite asynchronously </returns>
        public static void ClearRef(string url) => Reference<Sprite>.Clear(url);
    }
}