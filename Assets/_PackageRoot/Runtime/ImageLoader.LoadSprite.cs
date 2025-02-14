using System;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
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
        public static Future<Sprite> LoadSprite(string url, TextureFormat textureFormat = TextureFormat.ARGB32, bool ignoreImageNotFoundError = false, CancellationToken cancellationToken = default)
            => LoadSprite(url, new Vector2(0.5f, 0.5f), textureFormat, ignoreImageNotFoundError, cancellationToken);

        /// <summary>
        /// Load image from web or local path and return it as Sprite
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="pivot">Pivot of created Sprite</param>
        /// <param name="textureFormat">TextureFormat for the Texture2D creation</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the image was not found by specified url</param>
        /// <returns>Returns sprite asynchronously </returns>
        public static Future<Sprite> LoadSprite(string url, Vector2 pivot, TextureFormat textureFormat = TextureFormat.ARGB32, bool ignoreImageNotFoundError = false, CancellationToken cancellationToken = default)
        {
            var futureTexture = new FutureTexture(url, cancellationToken);
            var future = new FutureSprite(url, cancellationToken);

            futureTexture.PassEvents(future, texture => ToSprite(texture, pivot), passCancelled: true);

            futureTexture.StartLoading(
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
                        return texture;
                    return null;
                },
                parseWebRequest: webRequest
                    => DownloadHandlerTexture.GetContent(webRequest),
                ignoreImageNotFoundError);

            return future;
        }
    }
}