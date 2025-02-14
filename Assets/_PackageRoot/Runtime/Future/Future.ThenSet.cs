using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Extensions.Unity.ImageLoader
{
    public static partial class FutureEx
    {
        private static bool IsDestroyed(UIBehaviour uIBehaviour)
        {
            try
            {
                return uIBehaviour == null
                    || uIBehaviour.IsDestroyed()
                    || uIBehaviour.gameObject == null;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Set image into array of the generic target instances
        /// </summary>
        /// <param name="setter">Setter function that gets Target instance and Sprite instance, it should set the Sprite value into Target instance</param>
        /// <param name="targets">Array of generic Target instances</param>
        /// <returns>Returns async Future</returns>
        public static Future<Sprite> ThenSet<T>(this Future<Sprite> future, Action<T, Sprite> setter, params T[] targets)
        {
            if ((targets?.Length ?? 0) == 0)
            {
                future.FailToLoad(new Exception("No targets to set image"));
                return future;
            }

            return future.Then(sprite =>
            {
                UniTask.Post(() => // using only MainThread to set any images to any targets
                {
                    foreach (var target in targets)
                    {
                        if (target == null)
                        {
                            if (future.LogLevel.IsActive(DebugLevel.Warning))
                                Debug.LogWarning($"[ImageLoader] Future[id={future.id}] The target is null. Can't set image into it. Skipping.");
                            continue;
                        }
                        if (target is UIBehaviour uiBehaviour)
                        {
                            if (IsDestroyed(uiBehaviour))
                            {
                                if (future.LogLevel.IsActive(DebugLevel.Warning))
                                    Debug.LogWarning($"The target UIBehaviour is destroyed. Can't set image into it. Skipping.");
                                continue;
                            }
                        }
                        setter?.Invoke(target, sprite);
                    }
                });
            });
        }

        /// <summary>
        /// Set image into array of Images
        /// </summary>
        /// <param name="images">Array of Images</param>
        /// <returns>Returns async Future</returns>
        public static Future<Sprite> ThenSet(this Future<Sprite> future, params Image[] images)
            => future.ThenSet((target, sprite) => target.sprite = sprite, images);

        /// <summary>
        /// Set image into array of RawImages
        /// </summary>
        /// <param name="images">Array of RawImages</param>
        /// <returns>Returns async Future</returns>
        public static Future<Sprite> ThenSet(this Future<Sprite> future, params RawImage[] rawImages)
            => future.ThenSet((target, sprite) => target.texture = sprite?.texture, rawImages);

        /// <summary>
        /// Set image into array of SpriteRenderers
        /// </summary>
        /// <param name="images">Array of SpriteRenderers</param>
        /// <returns>Returns async Future</returns>
        public static Future<Sprite> ThenSet(this Future<Sprite> future, params SpriteRenderer[] spriteRenderers)
            => future.ThenSet((target, sprite) => target.sprite = sprite, spriteRenderers);

        /// <summary>
        /// Set image into array of Materials
        /// </summary>
        /// <param name="images">Array of Materials</param>
        /// <returns>Returns async Future</returns>
        public static Future<Sprite> ThenSet(this Future<Sprite> future, string propertyName = "_MainTex", params Material[] materials)
            => future.ThenSet((target, sprite) => target.SetTexture(propertyName, sprite?.texture), materials);
    }
}
