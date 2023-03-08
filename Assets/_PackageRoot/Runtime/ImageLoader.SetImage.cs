using System;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
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
                        if (settings.debugMode <= DebugMode.Exception)
                            Debug.LogException(e); 
                    }
                });
            }
            catch (Exception e) 
            { 
                if (settings.debugMode <= DebugMode.Exception)
                    Debug.LogException(e); 
            }
        }
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
                        if (settings.debugMode <= DebugMode.Exception)
                            Debug.LogException(e); 
                    }
                }
            });
        }
    }
}