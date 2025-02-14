using UnityEngine;
using System.Threading;
using UnityEngine.Networking;

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
            var future = new FutureSprite(url, cancellationToken);
            var futureRef = future.AsReference(DebugLevel.None);

            future.StartLoading(
                createWebRequest: requestUrl =>
                {
                    var webRequest = UnityWebRequestTexture.GetTexture(requestUrl);
                    webRequest.downloadHandler = new DownloadHandlerTexture(true);
                    return webRequest;
                },
                parseBytes: bytes =>
                {
                    var texture = new Texture2D(2, 2, textureFormat, true);
                    if (texture.LoadImage(bytes))
                        return ToSprite(texture, pivot);
                    return null;
                },
                parseWebRequest: webRequest =>
                {
                    var texture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;
                    return ToSprite(texture, pivot);
                },
                ignoreImageNotFoundError);

            return futureRef;
        }

        /// <summary>
        /// Clear all references to all loaded sprites
        /// </summary>
        public static void ClearRef() => Reference<Sprite>.Clear();

        /// <summary>
        /// Clear all references to a single loaded sprites by URL
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns operation status boolean</returns>
        public static bool ClearRef(string url) => Reference<Sprite>.Clear(url);
    }
}