using UnityEngine;
using System;
using UnityEngine.UI;

namespace Extensions.Unity.ImageLoader
{
    /// <summary>
    /// A generic tween implementation for float values with strongly typed context
    /// </summary>
    public class FloatTween<C> : Tween<C, float>
    {
        public FloatTween(C context, Func<C, float> getter, Action<C, float> setter,
                         float targetValue, float duration, TweenType tweenType = TweenType.Linear)
            : base(context, getter, setter, targetValue, duration, tweenType)
        {
        }

        public override void Update(float deltaTime)
        {
            if (!isPlaying) return;

            elapsedTime += deltaTime;
            var t = Mathf.Clamp01(elapsedTime / duration);
            var evaluatedT = Evaluate(t, tweenType);

            var currentValue = Mathf.Lerp(startValue, targetValue, evaluatedT);
            setter(context, currentValue);

            if (t >= 1f)
            {
                isPlaying = false;
            }
        }
    }

    /// <summary>
    /// A generic tween implementation for Vector2 values
    /// </summary>
    public class Vector2Tween<C> : Tween<C, Vector2>
    {
        public Vector2Tween(C context, Func<C, Vector2> getter, Action<C, Vector2> setter,
                           Vector2 targetValue, float duration, TweenType tweenType = TweenType.Linear)
            : base(context, getter, setter, targetValue, duration, tweenType)
        {
        }

        public override void Update(float deltaTime)
        {
            if (!isPlaying) return;

            elapsedTime += deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float evaluatedT = Evaluate(t, tweenType);

            Vector2 currentValue = Vector2.Lerp(startValue, targetValue, evaluatedT);
            setter(context, currentValue);

            if (t >= 1f)
            {
                isPlaying = false;
            }
        }

        public override void Dispose()
        {
            Stop();
            context = default;
            getter = null;
            setter = null;
        }
    }

    /// <summary>
    /// A generic tween implementation for Vector3 values
    /// </summary>
    public class Vector3Tween<C> : Tween<C, Vector3>
    {
        public Vector3Tween(C context, Func<C, Vector3> getter, Action<C, Vector3> setter,
                           Vector3 targetValue, float duration, TweenType tweenType = TweenType.Linear)
            : base(context, getter, setter, targetValue, duration, tweenType)
        {
        }

        public override void Update(float deltaTime)
        {
            if (!isPlaying) return;

            elapsedTime += deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float evaluatedT = Evaluate(t, tweenType);

            Vector3 currentValue = Vector3.Lerp(startValue, targetValue, evaluatedT);
            setter(context, currentValue);

            if (t >= 1f)
            {
                isPlaying = false;
            }
        }

        public override void Dispose()
        {
            Stop();
            context = default;
            getter = null;
            setter = null;
        }
    }

    /// <summary>
    /// A generic tween implementation for Color values
    /// </summary>
    public class ColorTween<C> : Tween<C, Color>
    {
        public ColorTween(C context, Func<C, Color> getter, Action<C, Color> setter,
                         Color targetValue, float duration, TweenType tweenType = TweenType.Linear)
            : base(context, getter, setter, targetValue, duration, tweenType)
        {
        }

        public override void Update(float deltaTime)
        {
            if (!isPlaying) return;

            elapsedTime += deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float evaluatedT = Evaluate(t, tweenType);

            Color currentValue = Color.Lerp(startValue, targetValue, evaluatedT);
            setter(context, currentValue);

            if (t >= 1f)
            {
                isPlaying = false;
            }
        }

        public override void Dispose()
        {
            Stop();
            context = default;
            getter = null;
            setter = null;
        }
    }

    /// <summary>
    /// A specialized FloatTween for Image component that provides convenience methods for common animations
    /// </summary>
    public class ImageFloatTween : FloatTween<Image>
    {
        /// <summary>
        /// Creates a tween for the Image's alpha value
        /// </summary>
        public static ImageFloatTween CreateAlphaTween(Image image, float targetAlpha, float duration,
                                                     TweenType tweenType = TweenType.Linear)
        {
            return new ImageFloatTween(
                image,
                img => img.color.a,
                (img, alpha) => {
                    Color c = img.color;
                    c.a = alpha;
                    img.color = c;
                },
                targetAlpha,
                duration,
                tweenType
            );
        }

        public ImageFloatTween(Image context, Func<Image, float> getter, Action<Image, float> setter,
                              float targetValue, float duration, TweenType tweenType = TweenType.Linear)
            : base(context, getter, setter, targetValue, duration, tweenType)
        {
        }
    }
}