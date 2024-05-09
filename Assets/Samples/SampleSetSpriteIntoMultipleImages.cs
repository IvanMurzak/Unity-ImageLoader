using Extensions.Unity.ImageLoader;
using UnityEngine;
using UnityEngine.UI;

public class SampleSetSpriteIntoMultipleImages : MonoBehaviour
{
    [SerializeField] string imageURL;
    [SerializeField] Image image1;
    [SerializeField] Image image2;

    void Start()
    {
        // Loading with auto set to multiple images 
        ImageLoader.LoadSprite(imageURL).ThenSet(image1, image2).Forget();
    }
}