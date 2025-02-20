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
        /// <param name="mipChain">Specifies whether mipmaps should be generated for the texture</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the image was not found by specified url</param>
        /// <returns>Returns sprite asynchronously </returns>
        public static IFuture<Reference<Sprite>> LoadSpriteRef(string url, TextureFormat textureFormat = TextureFormat.ARGB32, bool mipChain = true, bool ignoreImageNotFoundError = false, CancellationToken cancellationToken = default)
            => LoadSpriteRef(url, new Vector2(0.5f, 0.5f), textureFormat, mipChain, ignoreImageNotFoundError, cancellationToken);

        /// <summary>
        /// Load image from web or local path and return it as Sprite
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="pivot">Pivot of created Sprite</param>
        /// <param name="textureFormat">TextureFormat for the Texture2D creation</param>
        /// <param name="mipChain">Specifies whether mipmaps should be generated for the texture</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the image was not found by specified url</param>
        /// <returns>Returns sprite asynchronously</returns>
        public static IFuture<Reference<Sprite>> LoadSpriteRef(string url, Vector2 pivot, TextureFormat textureFormat = TextureFormat.ARGB32, bool mipChain = true, bool ignoreImageNotFoundError = false, CancellationToken cancellationToken = default)
        {
            var future = new FutureSprite(url, pivot, textureFormat, mipChain, cancellationToken);
            var futureRef = future.AsReference(settings.debugLevel);

            future.StartLoading(ignoreImageNotFoundError);

            return futureRef;
        }

        /// <summary>
        /// Clear all references to all loaded sprites
        /// </summary>
        public static void ClearSpriteRef() => Reference<Sprite>.Clear();

        /// <summary>
        /// Clear all references to a single loaded sprites by URL
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns operation status boolean</returns>
        public static bool ClearSpriteRef(string url) => Reference<Sprite>.Clear(url);
    }
}