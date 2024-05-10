using Extensions.Unity.ImageLoader;
using UnityEngine;
using UnityEngine.UI;

public class SampleLifecycle : MonoBehaviour
{
    [SerializeField] string imageURL;
    [SerializeField] Image image;

    void Start()
    {
        ImageLoader.LoadSprite(imageURL) // load sprite
            .LoadedFromMemoryCache(sprite => Debug.Log("Loaded from memory cache")) // if loaded from memory cache
            .LoadingFromDiskCache(() => Debug.Log("Loading from disk cache")) // if loading from disk cache
            .LoadedFromDiskCache(sprite => Debug.Log("Loaded from disk cache")) // if loaded from disk cache
            .LoadingFromSource(() => Debug.Log("Loading from source")) // if loading from source
            .LoadedFromSource(sprite => Debug.Log("Loaded from source")) // if loaded from source
            .Failed(exception => Debug.LogException(exception)) // if failed to load
            .Completed(isLoaded => Debug.Log($"Completed, isLoaded={isLoaded}")) // if completed  (failed, loaded or canceled)
            .Then(sprite => Debug.Log("Loaded")) // if loaded
            .ThenSet(image) // if loaded set sprite into image
            .Canceled(() => Debug.Log("Canceled")) // if cancelled
            .Forget();
    }
}