using Extensions.Unity.ImageLoader;
using UnityEngine;

public class SampleLoadTextureThenSetMaterial : MonoBehaviour
{
    [SerializeField] string imageURL; // URL of the image to be loaded
    [SerializeField] Material material; // Material to apply the loaded texture

    async void Start()
    {
        // Load a Texture2D from the web and cache it for faster loading next time
        material.mainTexture = await ImageLoader.LoadTexture(imageURL);

        // Load a Texture2D from the web and set it directly to the Material
        await ImageLoader.LoadTexture(imageURL).ThenSet(material);
    }
}