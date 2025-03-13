using Extensions.Unity.ImageLoader;
using UnityEngine;
using UnityEngine.UI;

public class SampleDecorator : MonoBehaviour
{
    [SerializeField] string url = "";
    [SerializeField] Image image;

    void Start()
    {
        ImageLoader.LoadSprite(url)
            .SetUseDiskCache(false)
            .SetUseMemoryCache(true);
    }
}