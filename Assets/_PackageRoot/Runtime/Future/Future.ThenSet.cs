using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Extensions.Unity.ImageLoader
{
    public static partial class FutureEx
    {
        /// <summary>
        /// Set image into array of the generic consumer instances. It uses MainThread to do the job.
        /// </summary>
        /// <param name="setter">Setter function that gets Consumer and loaded object, it should set the object into Consumer</param>
        /// <param name="consumers">Array of consumers for injecting</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<T> ThenSet<T, C>(this IFuture<T> future, Action<C, T> setter, params C[] consumers) => future.Then(obj =>
        {
            UniTask.Post(() => // using only MainThread to set any images to any targets
            {
                foreach (var target in consumers)
                {
                    if (target == null)
                    {
                        if (future.LogLevel.IsActive(DebugLevel.Warning))
                            Debug.LogWarning($"[ImageLoader] Future[id={future.Id}] The target is null. Can't set image into it. Skipping.");
                        continue;
                    }
                    if (target is UnityEngine.Object unityObject)
                    {
                        if (unityObject.IsNull())
                        {
                            if (future.LogLevel.IsActive(DebugLevel.Warning))
                                Debug.LogWarning($"The target ({typeof(T).Name}) is destroyed. Can't set image into it. Skipping.");
                            continue;
                        }
                    }
                    setter?.Invoke(target, obj);
                }
            });
        });

        /// <summary>
        /// Set image into array of Images
        /// </summary>
        /// <param name="images">Array of Images</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<Sprite> ThenSet(this IFuture<Sprite> future, params Image[] images)
            => future.ThenSet((consumer, sprite) => consumer.sprite = sprite, images);

        /// <summary>
        /// Set image into array of RawImages
        /// </summary>
        /// <param name="images">Array of RawImages</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<Sprite> ThenSet(this IFuture<Sprite> future, params RawImage[] rawImages)
            => future.ThenSet((consumer, sprite) => consumer.texture = sprite?.texture, rawImages);

        /// <summary>
        /// Set image into array of RawImages
        /// </summary>
        /// <param name="images">Array of RawImages</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<Texture2D> ThenSet(this IFuture<Texture2D> future, params RawImage[] rawImages)
            => future.ThenSet((consumer, texture) => consumer.texture = texture, rawImages);

        /// <summary>
        /// Set image into array of SpriteRenderers
        /// </summary>
        /// <param name="images">Array of SpriteRenderers</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<Sprite> ThenSet(this IFuture<Sprite> future, params SpriteRenderer[] spriteRenderers)
            => future.ThenSet((consumer, sprite) => consumer.sprite = sprite, spriteRenderers);

        /// <summary>
        /// Set image into array of Materials
        /// </summary>
        /// <param name="images">Array of Materials</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<Sprite> ThenSet(this IFuture<Sprite> future, string propertyName = "_MainTex", params Material[] materials)
            => future.ThenSet((consumer, sprite) => consumer.SetTexture(propertyName, sprite?.texture), materials);

        /// <summary>
        /// Set image into array of Materials
        /// </summary>
        /// <param name="images">Array of Materials</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<Texture2D> ThenSet(this IFuture<Texture2D> future, string propertyName = "_MainTex", params Material[] materials)
            => future.ThenSet((consumer, texture) => consumer.SetTexture(propertyName, texture), materials);
    }
}
