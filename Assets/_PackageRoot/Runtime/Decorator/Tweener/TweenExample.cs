using UnityEngine;
using UnityEngine.UI;

namespace Extensions.Unity.ImageLoader
{
    /// <summary>
    /// Example usage of the generic tweening system
    /// </summary>
    public class TweenExample : MonoBehaviour
    {
        [SerializeField] private Image targetImage;
        [SerializeField] private RectTransform targetTransform;
        [SerializeField] private float duration = 1.0f;
        [SerializeField] private Tween<Image, float>.TweenType tweenType = Tween<Image, float>.TweenType.EaseInOut;

        private ImageFloatTween alphaTween;
        private Vector2Tween<RectTransform> sizeTween;
        private Vector3Tween<RectTransform> positionTween;
        private ColorTween<Image> colorTween;

        private void Start()
        {
            if (targetImage == null)
                targetImage = GetComponent<Image>();

            if (targetTransform == null)
                targetTransform = GetComponent<RectTransform>();

            InitializeTweens();
        }

        private void InitializeTweens()
        {
            // Alpha tween example using the specialized ImageFloatTween
            alphaTween = TweenFactory.CreateImageAlpha(
                targetImage,
                1.0f, // Target alpha
                duration,
                tweenType
            );

            // Size tween example
            sizeTween = TweenFactory.CreateVector2(
                targetTransform,
                rt => rt.sizeDelta,
                (rt, size) => rt.sizeDelta = size,
                new Vector2(200, 200), // Target size
                duration,
                tweenType
            );

            // Position tween example
            positionTween = TweenFactory.CreateVector3(
                targetTransform,
                rt => rt.anchoredPosition3D,
                (rt, pos) => rt.anchoredPosition3D = pos,
                Vector3.zero, // Target position
                duration,
                tweenType
            );

            // Color tween example
            colorTween = TweenFactory.CreateImageColor(
                targetImage,
                Color.white, // Target color
                duration,
                tweenType
            );
        }

        private void Update()
        {
            // Update any active tweens
            if (alphaTween != null && alphaTween.IsPlaying())
                alphaTween.Update(Time.deltaTime);

            if (sizeTween != null && sizeTween.IsPlaying())
                sizeTween.Update(Time.deltaTime);

            if (positionTween != null && positionTween.IsPlaying())
                positionTween.Update(Time.deltaTime);

            if (colorTween != null && colorTween.IsPlaying())
                colorTween.Update(Time.deltaTime);
        }

        // Example methods to trigger tweens
        public void FadeIn()
        {
            // Use the specialized ImageFloatTween for alpha
            alphaTween = ImageFloatTween.CreateAlphaTween(
                targetImage,
                1.0f, // Fade in to 1.0
                duration,
                tweenType
            );
            alphaTween.Play();
        }

        public void FadeOut()
        {
            // Use the specialized ImageFloatTween for alpha
            alphaTween = ImageFloatTween.CreateAlphaTween(
                targetImage,
                0.0f, // Fade out to 0.0
                duration,
                tweenType
            );
            alphaTween.Play();
        }

        public void AnimateSize(Vector2 targetSize)
        {
            sizeTween = TweenFactory.CreateVector2(
                targetTransform,
                rt => rt.sizeDelta,
                (rt, size) => rt.sizeDelta = size,
                targetSize,
                duration,
                tweenType
            );
            sizeTween.Play();
        }

        public void AnimatePosition(Vector3 targetPosition)
        {
            positionTween = TweenFactory.CreateVector3(
                targetTransform,
                rt => rt.anchoredPosition3D,
                (rt, pos) => rt.anchoredPosition3D = pos,
                targetPosition,
                duration,
                tweenType
            );
            positionTween.Play();
        }

        public void AnimateColor(Color targetColor)
        {
            colorTween = TweenFactory.CreateImageColor(
                targetImage,
                targetColor,
                duration,
                tweenType
            );
            colorTween.Play();
        }

        // Example showing how to create a custom Image fill tween
        public void AnimateFill(float targetFill)
        {
            var fillTween = TweenFactory.CreateImageFill(
                targetImage,
                targetFill,
                duration,
                tweenType
            );
            fillTween.Play();

            // For simplicity in this example, we're not storing this tween in a field
            // In a real implementation, you might want to store it for disposal
        }

        private void OnDestroy()
        {
            // Clean up tweens
            alphaTween?.Dispose();
            sizeTween?.Dispose();
            positionTween?.Dispose();
            colorTween?.Dispose();
        }
    }
}