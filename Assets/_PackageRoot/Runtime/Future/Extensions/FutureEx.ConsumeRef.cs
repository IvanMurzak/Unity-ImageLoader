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
        public static IFuture<Reference<T>> Consume<T, C>(this IFuture<Reference<T>> future, Action<C, Reference<T>> setter, params C[] consumers) => future.Consume(reference =>
        {
            UniTask.Post(() => // using only MainThread to set any images to any targets
            {
                foreach (var consumer in consumers)
                {
                    if (ReferenceEquals(consumer, null) || consumer == null)
                    {
                        if (future.LogLevel.IsActive(DebugLevel.Warning))
                            Debug.LogWarning($"[ImageLoader] Future[id={future.Id}] The target is null. Can't set image into it. Skipping.");
                        continue;
                    }

                    Safe.Run(setter, consumer, reference, future.LogLevel);

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
        public static IFuture<Reference<Sprite>> Consume(this IFuture<Reference<Sprite>> future, params Image[] images)
            => future.Consume((consumer, reference) => consumer.sprite = reference?.Value, images);

        /// <summary>
        /// Set image into array of RawImages
        /// </summary>
        /// <param name="rawImages">Array of RawImages</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<Reference<Sprite>> Consume(this IFuture<Reference<Sprite>> future, params RawImage[] rawImages)
            => future.Consume((consumer, reference) => consumer.texture = reference?.Value?.texture, rawImages);

        /// <summary>
        /// Set image into array of RawImages
        /// </summary>
        /// <param name="rawImages">Array of RawImages</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<Reference<Texture2D>> Consume(this IFuture<Reference<Texture2D>> future, params RawImage[] rawImages)
            => future.Consume((consumer, reference) => consumer.texture = reference?.Value, rawImages);

        /// <summary>
        /// Set image into array of SpriteRenderers
        /// </summary>
        /// <param name="spriteRenderers">Array of SpriteRenderers</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<Reference<Sprite>> Consume(this IFuture<Reference<Sprite>> future, params SpriteRenderer[] spriteRenderers)
            => future.Consume((consumer, reference) => consumer.sprite = reference?.Value, spriteRenderers);

        /// <summary>
        /// Set image into array of Materials
        /// </summary>
        /// <param name="materials">Array of Materials</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<Reference<Sprite>> Consume(this IFuture<Reference<Sprite>> future, params Material[] materials)
            => future.Consume("_MainTex", materials);

        /// <summary>
        /// Set image into array of Materials
        /// </summary>
        /// <param name="propertyName">Property name to set the texture</param>
        /// <param name="materials">Array of Materials</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<Reference<Sprite>> Consume(this IFuture<Reference<Sprite>> future, string propertyName = "_MainTex", params Material[] materials)
            => future.Consume((consumer, reference) => consumer.SetTexture(propertyName, reference?.Value?.texture), materials);

        /// <summary>
        /// Set image into array of Materials
        /// </summary>
        /// <param name="materials">Array of Materials</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<Reference<Texture2D>> Consume(this IFuture<Reference<Texture2D>> future, params Material[] materials)
            => future.Consume("_MainTex", materials);

        /// <summary>
        /// Set image into array of Materials
        /// </summary>
        /// <param name="propertyName">Property name to set the texture</param>
        /// <param name="materials">Array of Materials</param>
        /// <returns>Returns async Future</returns>
        public static IFuture<Reference<Texture2D>> Consume(this IFuture<Reference<Texture2D>> future, string propertyName = "_MainTex", params Material[] materials)
            => future.Consume((consumer, reference) => consumer.SetTexture(propertyName, reference?.Value), materials);
    }
}
