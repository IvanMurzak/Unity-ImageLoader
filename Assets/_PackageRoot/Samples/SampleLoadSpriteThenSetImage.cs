using Extensions.Unity.ImageLoader;
using UnityEngine;
using UnityEngine.UI;

public class SampleLoadSpriteThenSetImage : MonoBehaviour
{
    [SerializeField] string imageURL; // URL of the image to be loaded
    [SerializeField] Image image; // UI Image component to display the loaded sprite

    async void Start()
    {
        // Load a sprite from the web and cache it for faster loading next time
        image.sprite = await ImageLoader.LoadSprite(imageURL);

        // Load a sprite from the web and set it directly to the Image component
        await ImageLoader.LoadSprite(imageURL).ThenSet(image);
    }
}