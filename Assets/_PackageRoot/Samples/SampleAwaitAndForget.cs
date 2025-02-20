using Extensions.Unity.ImageLoader;
using UnityEngine;
using UnityEngine.UI;

public class SampleAwaitAndForget : MonoBehaviour
{
    [SerializeField] string imageURL;
    [SerializeField] Image image;

    async void Start()
    {
        // Load image and wait
        await ImageLoader.LoadSprite(imageURL);

        // Load image, set image and wait
        await ImageLoader.LoadSprite(imageURL).ThenSet(image);

        // Let's skip waiting for completion.
        // To do that we can simply remove 'await' from the start.
        // To avoid compilation warning need to add '.Forget()'.
        ImageLoader.LoadSprite(imageURL).ThenSet(image).Forget();
    }
}