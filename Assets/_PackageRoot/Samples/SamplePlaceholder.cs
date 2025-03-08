using Extensions.Unity.ImageLoader;
using UnityEngine;
using UnityEngine.UI;

public class SamplePlaceholder : MonoBehaviour
{
    [SerializeField] string imageURL; // URL of the image to be loaded
    [SerializeField] Image image; // UI Image component to display the loaded sprite
    [SerializeField] Sprite placeholderAny;
    [SerializeField] Sprite placeholderLoadingFromSource;
    [SerializeField] Color placeholderFailedToLoad = Color.red;

    void Start()
    {
        ImageLoader.LoadSprite(imageURL)
            // set placeholder in all conditions
            .SetPlaceholder(placeholderAny)

            // set placeholder in a specific conditions
            .SetPlaceholder(placeholderLoadingFromSource, PlaceholderTrigger.LoadingFromSource)
            .SetPlaceholder(placeholderFailedToLoad, PlaceholderTrigger.FailedToLoad)

            // set consumer
            .Consume(image)
            .Forget();
    }
}