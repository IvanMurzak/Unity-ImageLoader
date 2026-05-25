using Extensions.Unity.ImageLoader;
using UnityEngine;
using UnityEngine.UI;

public class SampleReferences : MonoBehaviour
{
    [SerializeField] string imageURL;
    [SerializeField] Image image;

    void Start()
    {
        ImageLoader.LoadSpriteRef(imageURL) // load sprite using Reference
            .Consume(image) // if success set sprite into image, also creates binding to `image`
            .Forget();

        ImageLoader.LoadSpriteRef(imageURL) // load sprite using Reference
            .Loaded(reference => reference.DisposeOnDestroy(this))
            .Loaded(reference =>
            {
                var sprite = reference.Value;
                // use sprite
            })
            .Forget();

        var count = ImageLoader.GetReferenceCount(imageURL); // get count of references
    }
}