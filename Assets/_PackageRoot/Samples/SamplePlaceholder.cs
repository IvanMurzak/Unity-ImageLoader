using Extensions.Unity.ImageLoader;
using UnityEngine;
using UnityEngine.UI;

public class SamplePlaceholder : MonoBehaviour
{
    [SerializeField] string imageURL; // URL of the image to be loaded
    [SerializeField] Image image; // UI Image component to display the loaded sprite
    [SerializeField] Sprite placeholder1;
    [SerializeField] Sprite placeholder2;
    [SerializeField] Sprite placeholder3;

    void Start()
    {
        ImageLoader.LoadSprite(imageURL)
            // set placeholder in all conditions
            .SetPlaceholder(placeholder1)

            // set placeholder in a specific conditions
            .SetPlaceholder(placeholder2, PlaceholderTrigger.LoadingFromSource)
            .SetPlaceholder(placeholder3, PlaceholderTrigger.FailedToLoad)

            // set consumer
            .Consume(image)
            .Forget();
    }
}