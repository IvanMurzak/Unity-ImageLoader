using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Extensions.Unity.ImageLoader
{
    public static partial class FutureEx
    {
        // /// <summary>
        // /// Set image into array of the generic target instances
        // /// </summary>
        // /// <param name="setter">Setter function that gets Target instance and Sprite instance, it should set the Sprite value into Target instance</param>
        // /// <param name="targets">Array of generic Target instances</param>
        // /// <returns>Returns async Future</returns>
        // public static IFuture<Sprite> Setter<T>(this IFuture<Sprite> future, Action<T, Sprite> setter, params T[] targets)
        //     => future.Then(sprite =>
        //     {
        //         UniTask.Post(() => // using only MainThread to set any images to any targets
        //         {
        //             foreach (var target in targets)
        //             {
        //                 if (ReferenceEquals(target, null) || target == null || (target is UIBehaviour uiBehaviour && IsDestroyed(uiBehaviour)))
        //                 {
        //                     if (future.LogLevel.IsActive(DebugLevel.Warning))
        //                         Debug.LogWarning($"[ImageLoader] Future[id={future.Id}] The target is null. Can't set image into it. Skipping.");
        //                     continue;
        //                 }
        //                 Safe.Run(setter, target, sprite, future.LogLevel);
        //             }
        //         });
        //     });

        // /// <summary>
        // /// Set image into array of Images
        // /// </summary>
        // /// <param name="images">Array of Images</param>
        // /// <returns>Returns async Future</returns>
        // public static IFuture<Sprite> Setter(this IFuture<Sprite> future, params Image[] images)
        //     => future.Setter((target, sprite) => target.sprite = sprite, images);

        // /// <summary>
        // /// Set image into array of RawImages
        // /// </summary>
        // /// <param name="images">Array of RawImages</param>
        // /// <returns>Returns async Future</returns>
        // public static IFuture<Sprite> Setter(this IFuture<Sprite> future, params RawImage[] rawImages)
        //     => future.Setter((target, sprite) => target.texture = sprite?.texture, rawImages);

        // /// <summary>
        // /// Set image into array of SpriteRenderers
        // /// </summary>
        // /// <param name="images">Array of SpriteRenderers</param>
        // /// <returns>Returns async Future</returns>
        // public static IFuture<Sprite> Setter(this IFuture<Sprite> future, params SpriteRenderer[] spriteRenderers)
        //     => future.Setter((target, sprite) => target.sprite = sprite, spriteRenderers);

        // /// <summary>
        // /// Set image into array of Materials
        // /// </summary>
        // /// <param name="images">Array of Materials</param>
        // /// <returns>Returns async Future</returns>
        // public static IFuture<Sprite> Setter(this IFuture<Sprite> future, string propertyName = "_MainTex", params Material[] materials)
        //     => future.Setter((target, sprite) => target.SetTexture(propertyName, sprite?.texture), materials);
    }
}
