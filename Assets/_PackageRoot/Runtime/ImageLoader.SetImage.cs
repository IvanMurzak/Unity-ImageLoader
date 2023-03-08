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
        /// <returns>Returns async task</returns>
        public static async UniTask SetImage(string url, Image image)
        {
            try
            {
                if (image == null || image.IsDestroyed() || image.gameObject == null)
                    return;

                var sprite = await LoadSprite(url);
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
        /// <returns>Returns async task</returns>
        public static async UniTask SetImage(string url, params Image[] images)
        {
            if (images == null)
                return;

            var sprite = await LoadSprite(url);
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