using Extensions.Unity.ImageLoader;
using UnityEngine;
using UnityEngine.UI;

public class SampleErrorHandle : MonoBehaviour
{
    [SerializeField] string imageURL;
    [SerializeField] Image image;

    void Start()
    {
        ImageLoader.LoadSprite(imageURL) // load sprite
            .ThenSet(image) // if success set sprite into image
            .Failed(exception => Debug.LogException(exception)) // if fail print exception
            .Forget();

        ImageLoader.LoadSprite(imageURL) // load sprite
            .ThenSet(image) // if success set sprite into image
            .Then(sprite => image.gameObject.SetActive(true)) // if success activate gameObject
            .Failed(exception => image.gameObject.SetActive(false)) // if fail deactivate gameObject
            .Forget();
    }
}