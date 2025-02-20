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
        public static IFuture<Reference<T>> ThenSet<T, C>(this IFuture<Reference<T>> future, Action<C, Reference<T>> setter, params C[] consumers) => future.Then(reference =>
        {
            UniTask.Post(() => // using only MainThread to set any images to any targets
            {
                foreach (var consumer in consumers)
                {
                    if (consumer == null)
                    {
                        if (future.LogLevel.IsActive(DebugLevel.Warning))
                            Debug.LogWarning($"[ImageLoader] Future[id={future.Id}] The target is null. Can't set image into it. Skipping.");
                        continue;
                    }
                    if (consumer is UnityEngine.Object unityObject)
                    {
                        if (unityObject.IsNull())
                        {
                            if (future.LogLevel.IsActive(DebugLevel.Warning))
                                Debug.LogWarning($"The target ({typeof(T).Name}) is destroyed. Can't set image into it. Skipping.");
                            continue;
                        }
                    }

                    setter?.Invoke(consumer, reference);

                    // ┌────────────────────────┬─────────────────────────────────────────────────────┐
                    // │ Memory leak protection │ Connection Reference<T> to the Component            │
                    // └────────────────────────┘ It would be destroyed when the Component destroys   │
                    //                                                                                |
                    if (consumer is Component component)                                           // │
                        reference?.AddTo(component.GetCancellationTokenOnDestroy());               // │
                    // ───────────────────────────────────────────────────────────────────────────────┘
                }
            });
        });

        /// <summary>
        /// Set image into array of Images
        /// </summary>
        /// <param name="images">Array of Images</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<Reference<Sprite>> ThenSet(this IFuture<Reference<Sprite>> future, params Image[] images)
            => future.ThenSet((consumer, reference) => consumer.sprite = reference?.Value, images);

        /// <summary>
        /// Set image into array of RawImages
        /// </summary>
        /// <param name="images">Array of RawImages</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<Reference<Sprite>> ThenSet(this IFuture<Reference<Sprite>> future, params RawImage[] rawImages)
            => future.ThenSet((consumer, reference) => consumer.texture = reference?.Value?.texture, rawImages);

        /// <summary>
        /// Set image into array of RawImages
        /// </summary>
        /// <param name="images">Array of RawImages</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<Reference<Texture2D>> ThenSet(this IFuture<Reference<Texture2D>> future, params RawImage[] rawImages)
            => future.ThenSet((consumer, reference) => consumer.texture = reference?.Value, rawImages);

        /// <summary>
        /// Set image into array of SpriteRenderers
        /// </summary>
        /// <param name="images">Array of SpriteRenderers</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<Reference<Sprite>> ThenSet(this IFuture<Reference<Sprite>> future, params SpriteRenderer[] spriteRenderers)
            => future.ThenSet((consumer, reference) => consumer.sprite = reference?.Value, spriteRenderers);

        /// <summary>
        /// Set image into array of Materials
        /// </summary>
        /// <param name="images">Array of Materials</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<Reference<Sprite>> ThenSet(this IFuture<Reference<Sprite>> future, string propertyName = "_MainTex", params Material[] materials)
            => future.ThenSet((consumer, reference) => consumer.SetTexture(propertyName, reference?.Value?.texture), materials);

        /// <summary>
        /// Set image into array of Materials
        /// </summary>
        /// <param name="images">Array of Materials</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<Reference<Texture2D>> ThenSet(this IFuture<Reference<Texture2D>> future, string propertyName = "_MainTex", params Material[] materials)
            => future.ThenSet((consumer, reference) => consumer.SetTexture(propertyName, reference?.Value), materials);
    }
}
