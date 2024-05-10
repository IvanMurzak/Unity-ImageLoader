using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Extensions.Unity.ImageLoader
{
    public static partial class FutureEx
    {
        /// <summary>
        /// Set image into array of the generic target instances
        /// </summary>
        /// <param name="setter">Setter function that gets Target instance and Reference<Sprite> instance, it should set the Sprite value into Target instance</param>
        /// <param name="targets">Array of generic Target instances</param>
        /// <returns>Returns async Future</returns>
        public static Future<Reference<Sprite>> ThenSet<T>(this Future<Reference<Sprite>> future, Action<T, Reference<Sprite>> setter, params T[] targets)
        {
            if ((targets?.Length ?? 0) == 0)
            {
                future.FailToLoad(new Exception("No targets to set image"));
                return future;
            }

            return future.Then(reference =>
            {
                UniTask.Post(() => // using only MainThread to set any images to any targets
                {
                    foreach (var target in targets)
                    {
                        if (target == null)
                        {
                            if (ImageLoader.settings.debugLevel <= DebugLevel.Warning)
                                Debug.LogWarning($"[ImageLoader] The target is null. Can't set image into it. Skipping.");
                            continue;
                        }
                        if (target is UIBehaviour uiBehaviour)
                        {
                            if (IsDestroyed(uiBehaviour))
                            {
                                if (ImageLoader.settings.debugLevel <= DebugLevel.Warning)
                                    Debug.LogWarning($"The target UIBehaviour is destroyed. Can't set image into it. Skipping.");
                                continue;
                            }
                        }

                        setter?.Invoke(target, reference);

                        if (target is Component monoBehaviour)
                            reference.AddTo(monoBehaviour.GetCancellationTokenOnDestroy());
                    }
                });
            });
        }

        /// <summary>
        /// Set image into array of Images
        /// </summary>
        /// <param name="images">Array of Images</param>
        /// <returns>Returns async Future</returns>
        public static Future<Reference<Sprite>> ThenSet(this Future<Reference<Sprite>> future, params Image[] images)
            => future.ThenSet((target, reference) => target.sprite = reference.Value, images);

        /// <summary>
        /// Set image into array of RawImages
        /// </summary>
        /// <param name="images">Array of RawImages</param>
        /// <returns>Returns async Future</returns>
        public static Future<Reference<Sprite>> ThenSet(this Future<Reference<Sprite>> future, params RawImage[] rawImages)
            => future.ThenSet((target, reference) => target.texture = reference.Value.texture, rawImages);

        /// <summary>
        /// Set image into array of SpriteRenderers
        /// </summary>
        /// <param name="images">Array of SpriteRenderers</param>
        /// <returns>Returns async Future</returns>
        public static Future<Reference<Sprite>> ThenSet(this Future<Reference<Sprite>> future, params SpriteRenderer[] spriteRenderers)
            => future.ThenSet((target, reference) => target.sprite = reference.Value, spriteRenderers);

        /// <summary>
        /// Set image into array of Materials
        /// </summary>
        /// <param name="images">Array of Materials</param>
        /// <returns>Returns async Future</returns>
        public static Future<Reference<Sprite>> ThenSet(this Future<Reference<Sprite>> future, string propertyName = "_MainTex", params Material[] materials)
            => future.ThenSet((target, reference) => target.SetTexture(propertyName, reference.Value.texture), materials);
    }
}
