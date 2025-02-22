using Extensions.Unity.ImageLoader;
using UnityEngine;
using UnityEngine.UI;

public class SampleLoadSpriteThenSetIntoMultipleImages : MonoBehaviour
{
    [SerializeField] string imageURL; // URL of the image to be loaded
    [SerializeField] Image image1; // UI Image component to display the loaded sprite
    [SerializeField] Image image2; // UI Image component to display the loaded sprite

    void Start()
    {
        // Loading with auto set to multiple images
        ImageLoader.LoadSprite(imageURL).ThenSet(image1, image2).Forget();
    }
}