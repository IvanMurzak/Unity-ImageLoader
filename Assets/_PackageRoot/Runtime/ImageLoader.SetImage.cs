using System;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
        /// <summary>
        /// Load image from URL and set it to the Image component
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="image">Image component from Unity UI</param>
        /// <param name="textureFormat">TextureFormat for the Texture2D creation</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the image was not found by specified url</param>
        /// <returns>Returns async task</returns>
        public static UniTask SetSprite(string url, Image image, TextureFormat textureFormat = TextureFormat.ARGB32, bool ignoreImageNotFoundError = false)
            => SetSprite(url, image, Vector2.one * 0.5f, textureFormat, ignoreImageNotFoundError);

        /// <summary>
        /// Load image from URL and set it to the Image component
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="image">Image component from Unity UI</param>
        /// <param name="pivot">Pivot of created Sprite</param>
        /// <param name="textureFormat">TextureFormat for the Texture2D creation</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the image was not found by specified url</param>
        /// <returns>Returns async task</returns>
        public static async UniTask SetSprite(string url, Image image, Vector2 pivot, TextureFormat textureFormat = TextureFormat.ARGB32, bool ignoreImageNotFoundError = false)
        {
            try
            {
                if (image == null || image.IsDestroyed() || image.gameObject == null)
                    return;

                var sprite = await LoadSprite(url, pivot, textureFormat, ignoreImageNotFoundError);
                UniTask.Post(() =>
                {
                    if (image == null || image.IsDestroyed() || GameObject.Equals(image.gameObject, null))
                        return;
                    try
                    {
                        image.sprite = sprite;
                    }
                    catch (Exception e)
                    {
                        if (settings.debugLevel <= DebugLevel.Exception)
                            Debug.LogException(e);
                    }
                });
            }
            catch (Exception e) 
            { 
                if (settings.debugLevel <= DebugLevel.Exception)
                    Debug.LogException(e); 
            }
        }

        /// <summary>
        /// Load image from URL and set it to the Image components
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="images">Array of Image components from Unity UI</param>
        /// <returns>Returns async task</returns>
        public static UniTask SetSprite(string url, params Image[] images)
            => SetSprite(url, Vector2.one * 0.5f, TextureFormat.ARGB32, false, images);

        /// <summary>
        /// Load image from URL and set it to the Image components
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="textureFormat">TextureFormat for the Texture2D creation</param>
        /// <param name="images">Array of Image components from Unity UI</param>
        /// <returns>Returns async task</returns>
        public static UniTask SetSprite(string url, TextureFormat textureFormat, params Image[] images)
            => SetSprite(url, Vector2.one * 0.5f, textureFormat, false, images);

        /// <summary>
        /// Load image from URL and set it to the Image components
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the image was not found by specified url</param>
        /// <param name="images">Array of Image components from Unity UI</param>
        /// <returns>Returns async task</returns>
        public static UniTask SetSprite(string url, bool ignoreImageNotFoundError, params Image[] images)
            => SetSprite(url, Vector2.one * 0.5f, TextureFormat.ARGB32, ignoreImageNotFoundError, images);

        /// <summary>
        /// Load image from URL and set it to the Image components
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="textureFormat">TextureFormat for the Texture2D creation</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the image was not found by specified url</param>
        /// <param name="images">Array of Image components from Unity UI</param>
        /// <returns>Returns async task</returns>
        public static UniTask SetSprite(string url, TextureFormat textureFormat, bool ignoreImageNotFoundError, params Image[] images)
            => SetSprite(url, Vector2.one * 0.5f, textureFormat, ignoreImageNotFoundError, images);

        /// <summary>
        /// Load image from URL and set it to the Image components
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="pivot">Pivot of created Sprite</param>
        /// <param name="textureFormat">TextureFormat for the Texture2D creation</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the image was not found by specified url</param>
        /// <param name="images">Array of Image components from Unity UI</param>
        /// <returns>Returns async task</returns>
        public static async UniTask SetSprite(string url, Vector2 pivot, TextureFormat textureFormat = TextureFormat.ARGB32, bool ignoreImageNotFoundError = false, params Image[] images)
        {
            if (images == null)
                return;

            var sprite = await LoadSprite(url, pivot, textureFormat, ignoreImageNotFoundError);
            UniTask.Post(() =>
            {
                for (var i = 0; i < images.Length; i++)
                {
                    try
                    {
                        if (images[i] == null || images[i].IsDestroyed() || GameObject.Equals(images[i].gameObject, null))
                            continue;

                        images[i].sprite = sprite;
                    }
                    catch (Exception e)
                    {
                        if (settings.debugLevel <= DebugLevel.Exception)
                            Debug.LogException(e); 
                    }
                }
            });
        }
    }
}