using UnityEngine;
using System;
using UnityEngine.UI;

namespace Extensions.Unity.ImageLoader
{
    /// <summary>
    /// Factory class for creating tweens with a simplified interface
    /// </summary>
    public static class TweenFactory
    {
        /// <summary>
        /// Creates a tween that animates a float property
        /// </summary>
        /// <typeparam name="T">The type of the context object</typeparam>
        /// <param name="context">The object that contains the property to animate</param>
        /// <param name="getter">Function to get the current value</param>
        /// <param name="setter">Function to set the new value</param>
        /// <param name="targetValue">The target value to animate to</param>
        /// <param name="duration">Duration of the animation in seconds</param>
        /// <param name="tweenType">The easing function to use</param>
        /// <returns>A tween that can be played</returns>
        public static FloatTween<T> CreateFloat<T>(T context, Func<T, float> getter, Action<T, float> setter,
                                               float targetValue, float duration,
                                               Tween<T, float>.TweenType tweenType = Tween<T, float>.TweenType.Linear)
        {
            return new FloatTween<T>(
                context,
                getter,
                setter,
                targetValue,
                duration,
                tweenType
            );
        }

        /// <summary>
        /// Creates a tween that animates a Vector2 property
        /// </summary>
        public static Vector2Tween<T> CreateVector2<T>(T context, Func<T, Vector2> getter, Action<T, Vector2> setter,
                                                  Vector2 targetValue, float duration,
                                                  Tween<T, Vector2>.TweenType tweenType = Tween<T, Vector2>.TweenType.Linear)
        {
            return new Vector2Tween<T>(
                context,
                getter,
                setter,
                targetValue,
                duration,
                tweenType
            );
        }

        /// <summary>
        /// Creates a tween that animates a Vector3 property
        /// </summary>
        public static Vector3Tween<T> CreateVector3<T>(T context, Func<T, Vector3> getter, Action<T, Vector3> setter,
                                                  Vector3 targetValue, float duration,
                                                  Tween<T, Vector3>.TweenType tweenType = Tween<T, Vector3>.TweenType.Linear)
        {
            return new Vector3Tween<T>(
                context,
                getter,
                setter,
                targetValue,
                duration,
                tweenType
            );
        }

        /// <summary>
        /// Creates a tween that animates a Color property
        /// </summary>
        public static ColorTween<T> CreateColor<T>(T context, Func<T, Color> getter, Action<T, Color> setter,
                                              Color targetValue, float duration,
                                              Tween<T, Color>.TweenType tweenType = Tween<T, Color>.TweenType.Linear)
        {
            return new ColorTween<T>(
                context,
                getter,
                setter,
                targetValue,
                duration,
                tweenType
            );
        }

        /// <summary>
        /// Creates a tween that animates an Image's alpha value
        /// </summary>
        public static ImageFloatTween CreateImageAlpha(Image image, float targetAlpha, float duration,
                                                   Tween<Image, float>.TweenType tweenType = Tween<Image, float>.TweenType.Linear)
        {
            return ImageFloatTween.CreateAlphaTween(image, targetAlpha, duration, tweenType);
        }

        /// <summary>
        /// Creates a tween that animates an Image's fill amount
        /// </summary>
        public static FloatTween<Image> CreateImageFill(Image image, float targetFill, float duration,
                                                  Tween<Image, float>.TweenType tweenType = Tween<Image, float>.TweenType.Linear)
        {
            return new FloatTween<Image>(
                image,
                img => img.fillAmount,
                (img, fill) => img.fillAmount = fill,
                targetFill,
                duration,
                tweenType
            );
        }

        /// <summary>
        /// Creates a tween that animates an Image's color
        /// </summary>
        public static ColorTween<Image> CreateImageColor(Image image, Color targetColor, float duration,
                                                   Tween<Image, Color>.TweenType tweenType = Tween<Image, Color>.TweenType.Linear)
        {
            return new ColorTween<Image>(
                image,
                img => img.color,
                (img, color) => img.color = color,
                targetColor,
                duration,
                tweenType
            );
        }
    }
}