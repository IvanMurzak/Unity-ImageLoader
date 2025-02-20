using Extensions.Unity.ImageLoader;
using UnityEngine;
using UnityEngine.UI;

public class SampleImageLoading : MonoBehaviour
{
    [SerializeField] string imageURL;
    [SerializeField] Image image;
    [SerializeField] Material material;

    async void Start()
    {
        // Loading Sprite from web, cached for quick load next time
        image.sprite = await ImageLoader.LoadSprite(imageURL);

        // Same loading with self inject into image
        await ImageLoader.LoadSprite(imageURL).ThenSet(image);

        // Loading Texture2D from web, cached for quick load next time
        material.mainTexture = await ImageLoader.LoadTexture(imageURL);

        // Same loading with self inject into material
        await ImageLoader.LoadTexture(imageURL).ThenSet(material);
    }
}