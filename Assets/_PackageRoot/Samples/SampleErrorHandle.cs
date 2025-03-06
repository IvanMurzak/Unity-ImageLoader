using Extensions.Unity.ImageLoader;
using UnityEngine;
using UnityEngine.UI;

public class SampleErrorHandle : MonoBehaviour
{
    [SerializeField] string imageURL; // URL of the image to be loaded
    [SerializeField] Image image; // UI Image component to display the loaded sprite

    void Start()
    {
        ImageLoader.LoadSprite(imageURL) // Attempt to load a sprite
            .Consume(image) // If successful, set the sprite to the Image component
            .Failed(exception => Debug.LogException(exception)) // If an error occurs, log the exception
            .Forget(); // Forget the task to avoid compilation warning

        ImageLoader.LoadSprite(imageURL) // Attempt to load a sprite
            .Consume(image) // If successful, set the sprite to the Image component
            .Loaded(sprite => image.gameObject.SetActive(true)) // If successful, activate the GameObject
            .Failed(exception => image.gameObject.SetActive(false)) // If an error occurs, deactivate the GameObject
            .Forget(); // Forget the task to avoid compilation warning
    }
}