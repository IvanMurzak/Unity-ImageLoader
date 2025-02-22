using UnityEngine;
using System.Threading;

namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
        /// <summary>
        /// Load image from web or local path and return it as Texture
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="pivot">Pivot of created Texture</param>
        /// <param name="textureFormat">TextureFormat for the Texture2D creation</param>
        /// <param name="mipChain">Specifies whether mipmaps should be generated for the texture</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the image was not found by specified url</param>
        /// <returns>Returns texture asynchronously </returns>
        public static FutureTexture LoadTexture(string url, TextureFormat textureFormat = TextureFormat.ARGB32, bool mipChain = true, bool ignoreImageNotFoundError = false, CancellationToken cancellationToken = default)
        {
            var future = new FutureTexture(url, textureFormat, mipChain, cancellationToken);
            future.StartLoading(ignoreImageNotFoundError);
            return future;
        }
    }
}