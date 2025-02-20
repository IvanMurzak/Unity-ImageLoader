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
            .ThenSet(image) // if success set sprite into image, also creates binding to `image`
            .Forget();

        ImageLoader.LoadSpriteRef(imageURL) // load sprite using Reference
            .Then(reference => reference.DisposeOnDestroy(this))
            .Then(reference =>
            {
                var sprite = reference.Value;
                // use sprite
            })
            .Forget();

        var count = ImageLoader.GetReferenceCount(imageURL); // get count of references
    }
}