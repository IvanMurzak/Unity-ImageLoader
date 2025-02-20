using Extensions.Unity.ImageLoader;
using UnityEngine;
using UnityEngine.UI;

public class SampleLifecycle : MonoBehaviour
{
    [SerializeField] string imageURL; // URL of the image to be loaded
    [SerializeField] Image image; // UI Image component to display the loaded sprite

    void Start()
    {
        ImageLoader.LoadSprite(imageURL) // loading process started
            // ┌──────────────────────────┬────────────────────────────────────────────────────────────────────────┐
            // │ Loading lifecycle events │                                                                        │
            // └──────────────────────────┘                                                                        │
            .LoadedFromMemoryCache(sprite => Debug.Log("Loaded from memory cache")) // on loaded from memory cache │
            .LoadingFromDiskCache (()     => Debug.Log("Loading from disk cache"))  // on loading from disk cache  │
            .LoadedFromDiskCache  (sprite => Debug.Log("Loaded from disk cache"))   // on loaded from disk cache   │
            .LoadingFromSource    (()     => Debug.Log("Loading from source"))      // on loading from source      │
            .LoadedFromSource     (sprite => Debug.Log("Loaded from source"))       // on loaded from source       │
            // ────────────────────────────────────────────────────────────────────────────────────────────────────┘

            // ┌───────────────────────────┬──────────────────────────────────────────┐
            // │ Negative lifecycle events │                                          │
            // └───────────────────────────┘                                          │
            .Canceled(() => Debug.Log("Canceled"))              // on canceled        │
            .Failed(exception => Debug.LogException(exception)) // on failed to load  │
            // ───────────────────────────────────────────────────────────────────────┘

            // ┌──────────────────────────────────────┬──────────────────────────────┐
            // │ Successfully loaded lifecycle events │                              │
            // └──────────────────────────────────────┘                              │
            .Then(sprite => Debug.Log("Loaded")) // on loaded                        │
            .ThenSet(image)                      // on loaded set sprite into image  │
            // ──────────────────────────────────────────────────────────────────────┘

            // ┌──────────────────────┬──────────────────────────────────────────────────────────────────────────┐
            // │ The end of lifecycle │                                                                          │
            // └──────────────────────┘                                                                          │
            .Completed(isLoaded => Debug.Log($"Completed, isLoaded={isLoaded}")) // on completed                 │
            //                                                                   // [loaded, failed or canceled] │
            // ──────────────────────────────────────────────────────────────────────────────────────────────────┘

            .Forget(); // removes the compilation warning, does nothing else
    }
}