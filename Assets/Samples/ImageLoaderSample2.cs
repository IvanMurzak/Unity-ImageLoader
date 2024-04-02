using Extensions.Unity.ImageLoader;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ImageLoaderSample2 : MonoBehaviour
{
    [SerializeField] string imageURL;
    [SerializeField] Image image1;
    [SerializeField] Image image2;

    void Start()
    {
        // Loading with auto set to image
        ImageLoader.SetSprite(imageURL, image1, image2).Forget();
    }
}