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
        /// <returns>Returns async task</returns>
        public static async UniTask SetSpriteRenderer(string url, SpriteRenderer spriteRenderer)
        {
            try
            {
                if (spriteRenderer == null || spriteRenderer.gameObject == null)
                    return;

                var sprite = await LoadSprite(url);
                UniTask.Post(() =>
                {
                    if (spriteRenderer == null || GameObject.Equals(spriteRenderer.gameObject, null))
                        return;
                    try
                    {
                        spriteRenderer.sprite = sprite;
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
        /// Load image from URL and set it to the SpriteRenderer components
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns async task</returns>
        public static async UniTask SetImage(string url, params SpriteRenderer[] spriteRenderers)
        {
            if (spriteRenderers == null)
                return;

            var sprite = await LoadSprite(url);
            UniTask.Post(() =>
            {
                for (var i = 0; i < spriteRenderers.Length; i++)
                {
                    try
                    {
                        if (spriteRenderers[i] == null || GameObject.Equals(spriteRenderers[i].gameObject, null))
                            continue;

                        spriteRenderers[i].sprite = sprite;
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