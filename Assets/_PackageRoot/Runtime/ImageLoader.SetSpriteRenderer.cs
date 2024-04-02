using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
        /// <summary>
        /// Load image from URL and set it to the SpriteRenderer component
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="spriteRenderer">SpriteRenderer components from Unity</param>
        /// <param name="textureFormat">TextureFormat for the Texture2D creation</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the image was not found by specified url</param>
        /// <returns>Returns async task</returns>
        public static UniTask SetSprite(string url, SpriteRenderer spriteRenderer, TextureFormat textureFormat = TextureFormat.ARGB32, bool ignoreImageNotFoundError = false)
            => SetSprite(url, spriteRenderer, Vector2.one * 0.5f, textureFormat, ignoreImageNotFoundError);

        /// <summary>
        /// Load image from URL and set it to the SpriteRenderer component
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="spriteRenderer">SpriteRenderer components from Unity</param>
        /// <param name="pivot">Pivot of created Sprite</param>
        /// <param name="textureFormat">TextureFormat for the Texture2D creation</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the image was not found by specified url</param>
        /// <returns>Returns async task</returns>
        public static async UniTask SetSprite(string url, SpriteRenderer spriteRenderer, Vector2 pivot, TextureFormat textureFormat = TextureFormat.ARGB32, bool ignoreImageNotFoundError = false)
        {
            try
            {
                if (spriteRenderer == null || spriteRenderer.gameObject == null)
                    return;

                var sprite = await LoadSprite(url, pivot, textureFormat, ignoreImageNotFoundError);
                UniTask.Post(() =>
                {
                    if (spriteRenderer == null || GameObject.Equals(spriteRenderer.gameObject, null))
                        return;
                    try
                    {
                        spriteRenderer.sprite = sprite.Value;
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
        /// <param name="spriteRenderers">Array of SpriteRenderer components from Unity</param>
        /// <returns>Returns async task</returns>
        public static UniTask SetSprite(string url, params SpriteRenderer[] spriteRenderers)
            => SetSprite(url, Vector2.one * 0.5f, TextureFormat.ARGB32, false, spriteRenderers);

        /// <summary>
        /// Load image from URL and set it to the Image components
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="textureFormat">TextureFormat for the Texture2D creation</param>
        /// <param name="spriteRenderers">Array of SpriteRenderer components from Unity</param>
        /// <returns>Returns async task</returns>
        public static UniTask SetSprite(string url, TextureFormat textureFormat, params SpriteRenderer[] spriteRenderers)
            => SetSprite(url, Vector2.one * 0.5f, textureFormat, false, spriteRenderers);

        /// <summary>
        /// Load image from URL and set it to the Image components
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the image was not found by specified url</param>
        /// <param name="spriteRenderers">Array of SpriteRenderer components from Unity</param>
        /// <returns>Returns async task</returns>
        public static UniTask SetSprite(string url, bool ignoreImageNotFoundError, params SpriteRenderer[] spriteRenderers)
            => SetSprite(url, Vector2.one * 0.5f, TextureFormat.ARGB32, ignoreImageNotFoundError, spriteRenderers);

        /// <summary>
        /// Load image from URL and set it to the Image components
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="textureFormat">TextureFormat for the Texture2D creation</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the image was not found by specified url</param>
        /// <param name="spriteRenderers">Array of SpriteRenderer components from Unity</param>
        /// <returns>Returns async task</returns>
        public static UniTask SetSprite(string url, TextureFormat textureFormat, bool ignoreImageNotFoundError, params SpriteRenderer[] spriteRenderers)
            => SetSprite(url, Vector2.one * 0.5f, textureFormat, ignoreImageNotFoundError, spriteRenderers);

        /// <summary>
        /// Load image from URL and set it to the SpriteRenderer components
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="pivot">Pivot of created Sprite</param>
        /// <param name="textureFormat">TextureFormat for the Texture2D creation</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the image was not found by specified url</param>
        /// <param name="spriteRenderers">Array of SpriteRenderer components from Unity</param>
        /// <returns>Returns async task</returns>
        public static async UniTask SetSprite(string url, Vector2 pivot, TextureFormat textureFormat = TextureFormat.ARGB32, bool ignoreImageNotFoundError = false, params SpriteRenderer[] spriteRenderers)
        {
            if (spriteRenderers == null)
                return;

            var sprite = await LoadSprite(url, pivot, textureFormat, ignoreImageNotFoundError);
            UniTask.Post(() =>
            {
                for (var i = 0; i < spriteRenderers.Length; i++)
                {
                    try
                    {
                        if (spriteRenderers[i] == null || GameObject.Equals(spriteRenderers[i].gameObject, null))
                            continue;

                        spriteRenderers[i].sprite = sprite.Value;
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