using Cysharp.Threading.Tasks;
using Extensions.Unity.ImageLoader;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class SampleCancellation : MonoBehaviour
{
    [SerializeField] string imageURL;
    [SerializeField] Image image;

    void Start()
    {
        ImageLoader.LoadSprite(imageURL) // load sprite
            .Consume(image) // if success set sprite into image
            .CancelOnDestroy(this) // cancel OnDestroy event of current gameObject
            .Forget();

        ImageLoader.LoadSprite(imageURL) // load sprite
            .Consume(image) // if success set sprite into image
            .Failed(exception => Debug.LogException(exception)) // if fail print exception
            .CancelOnDestroy(this) // cancel OnDestroy event of current gameObject
            .Forget();

        ImageLoader.LoadSprite(imageURL) // load sprite
            .Consume(image) // if success set sprite into image
            .Loaded(sprite => image.gameObject.SetActive(true)) // if success activate gameObject
            .Failed(exception => image.gameObject.SetActive(false)) // if fail deactivate gameObject
            .Canceled(() => Debug.Log("ImageLoading canceled")) // if cancelled
            .CancelOnDisable(this) // cancel OnDisable event of current gameObject
            .Forget();
    }

    void DestroyWithMonoBehaviour()
    {
        ImageLoader.LoadSprite(imageURL)
            .Consume(image)
            .CancelOnEnable(this)   // cancel on OnEnable event of current MonoBehaviour
            .CancelOnDisable(this)  // cancel on OnDisable event of current MonoBehaviour
            .CancelOnDestroy(this); // cancel on OnDestroy event of current MonoBehaviour
    }

    void SimpleCancellation()
    {
        var future = ImageLoader.LoadSprite(imageURL).Consume(image);
        future.Cancel();
    }

    void CancellationTokenSample1()
    {
        var cancellationTokenSource = new CancellationTokenSource();

        // loading with attached cancellation token
        ImageLoader.LoadSprite(imageURL, cancellationToken: cancellationTokenSource.Token)
            .Consume(image)
            .Forget();

        cancellationTokenSource.Cancel(); // canceling
    }

    void CancellationTokenSample2()
    {
        var cancellationTokenSource = new CancellationTokenSource();

        ImageLoader.LoadSprite(imageURL)
            .Consume(image)
            .Register(cancellationTokenSource.Token) // registering cancellation token
            .Forget();

        cancellationTokenSource.Cancel(); // canceling
    }

    void DisposeSample()
    {
        using (var future = ImageLoader.LoadSprite(imageURL).Consume(image))
        {
            // future would be canceled and disposed outside of the brackets
        }
    }
}