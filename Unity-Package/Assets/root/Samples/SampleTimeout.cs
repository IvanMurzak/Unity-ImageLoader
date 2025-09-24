using Extensions.Unity.ImageLoader;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SampleTimeout : MonoBehaviour
{
    [SerializeField] string imageURL;
    [SerializeField] Image image;

    void Start()
    {
        ImageLoader.LoadSprite(imageURL) // load sprite
            .Consume(image) // if success set sprite into image
            .Timeout(TimeSpan.FromSeconds(10)) // set timeout duration 10 seconds
            .Forget();
    }
}