using UnityEngine;
using System.Threading;

namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
        /// <summary>
        /// Load image from web or local path and return it as Texture2D
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="textureFormat">TextureFormat for the Texture2D creation</param>
        /// <param name="mipChain">Specifies whether mipmaps should be generated for the texture</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the image was not found by specified url</param>
        /// <returns>Returns texture asynchronously</returns>
        public static IFuture<Reference<Texture2D>> LoadTextureRef(string url, TextureFormat textureFormat = TextureFormat.ARGB32, bool mipChain = true, bool ignoreImageNotFoundError = false, CancellationToken cancellationToken = default)
        {
            var future = new FutureTexture(url, textureFormat, mipChain, cancellationToken);
            var futureRef = future.AsReference(settings.debugLevel);

            future.StartLoading(ignoreImageNotFoundError);

            return futureRef;
        }

        /// <summary>
        /// Clear all references to all loaded textures
        /// </summary>
        public static void ClearTextureRef() => Reference<Texture>.Clear();

        /// <summary>
        /// Clear all references to a single loaded textures by URL
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns operation status boolean</returns>
        public static bool ClearTextureRef(string url) => Reference<Texture>.Clear(url);
    }
}