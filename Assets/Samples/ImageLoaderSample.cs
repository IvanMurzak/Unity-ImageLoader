using Extensions.Unity.ImageLoader;
using UnityEngine;
using UnityEngine.UI;

public class ImageLoaderSample : MonoBehaviour
{
    [SerializeField] string imageURL;
    [SerializeField] Image image;

    async void Start()
    {
        // Loading sprite from web, cached for quick load next time
        image.sprite = await ImageLoader.LoadSprite(imageURL);

        // Same loading with auto set to image
        await ImageLoader.SetSprite(imageURL, image);
    }
}