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
        public static FutureSprite LoadSprite(string url, float pixelDensity = 100, TextureFormat textureFormat = TextureFormat.ARGB32, bool mipChain = true, bool ignoreImageNotFoundError = false, CancellationToken cancellationToken = default)
            => LoadSprite(url, new Vector2(0.5f, 0.5f), pixelDensity, textureFormat, ignoreImageNotFoundError, mipChain, cancellationToken);

        /// <summary>
        /// Load image from web or local path and return it as Sprite
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="pivot">Pivot of created Sprite</param>
        /// <param name="textureFormat">TextureFormat for the Texture2D creation</param>
        /// <param name="mipChain">Specifies whether mipmaps should be generated for the texture</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the image was not found by specified url</param>
        /// <returns>Returns sprite asynchronously </returns>
        public static FutureSprite LoadSprite(string url, Vector2 pivot, float pixelDensity = 100, TextureFormat textureFormat = TextureFormat.ARGB32, bool mipChain = true, bool ignoreImageNotFoundError = false, CancellationToken cancellationToken = default)
        {
            var future = new FutureSprite(url, pivot, pixelDensity, textureFormat, mipChain, cancellationToken);
            future.StartLoading(ignoreImageNotFoundError);
            return future;
        }
    }
}