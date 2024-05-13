# Unity Image Loader

![npm](https://img.shields.io/npm/v/extensions.unity.imageloader) [![openupm](https://img.shields.io/npm/v/extensions.unity.imageloader?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/extensions.unity.imageloader/) ![License](https://img.shields.io/github/license/IvanMurzak/Unity-ImageLoader) [![Stand With Ukraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/badges/StandWithUkraine.svg)](https://stand-with-ukraine.pp.ua)

Async image loader with two caching layers for Unity.

## Features

- ✔️ Async loading from **Web** or **Local** `ImageLoader.LoadSprite(imageURL);`
- ✔️ **Memory** and **Disk** caching - tries to load from memory first, then from disk
- ✔️ Dedicated thread for disk operations
- ✔️ Avoids loading same image multiple times simultaneously, task waits for completion the first and just returns loaded image if at least one cache layer activated
- ✔️ Auto set to Image `ImageLoader.LoadSprite(imageURL).ThenSet(image);`
- ✔️ Auto set to RawImage `ImageLoader.LoadSprite(imageURL).ThenSet(rawImage);`
- ✔️ Auto set to Material `ImageLoader.LoadSprite(imageURL).ThenSet(material, "_MainTex");`
- ✔️ Auto set to SpriteRenderer `ImageLoader.LoadSprite(imageURL).ThenSet(spriteRenderer);`
- ✔️ Cancellation `ImageLoader.LoadSprite(imageURL).Cancel();`
- ✔️ Cancellation handling `ImageLoader.LoadSprite(imageURL).Cancelled(() => ...);`
- ✔️ Error handling `ImageLoader.LoadSprite(imageURL).Failed(exception => ...);`
- ✔️ Debug level for logging `ImageLoader.settings.debugLevel = DebugLevel.Error;`

# Installation

- [Install OpenUPM-CLI](https://github.com/openupm/openupm-cli#installation)
- Open command line in Unity project folder
- Run the command

``` CLI
openupm add extensions.unity.imageloader
```

# Usage

In the main thread somewhere at the start of the project need to call `ImageLoader.Init();` once to initialize static properties in the right thread. It is required to make in the main thread. Then you can use `ImageLoader` from any thread and at any time.

## Sample - Loading Sprite, set to Image

``` C#
using Extensions.Unity.ImageLoader;
using UnityEngine;
using UnityEngine.UI;

public class SampleImageLoading : MonoBehaviour
{
    [SerializeField] string imageURL;
    [SerializeField] Image image;

    async void Start()
    {
        // Loading sprite from web, cached for quick load next time
        image.sprite = await ImageLoader.LoadSprite(imageURL);

        // Same loading with auto set to image
        await ImageLoader.LoadSprite(imageURL).ThenSet(image);
    }
}
```

## Sample - Loading image into multiple Image components

``` C#
using Extensions.Unity.ImageLoader;
using UnityEngine;
using UnityEngine.UI;

public class SampleSetMultipleSpriteIntoMultipleImages : MonoBehaviour
{
    [SerializeField] string imageURL;
    [SerializeField] Image image1;
    [SerializeField] Image image2;

    void Start()
    {
        // Loading with auto set to multiple images 
        ImageLoader.LoadSprite(imageURL).ThenSet(image1, image2).Forget();
    }
}
```

## Sample - Error handling

``` C#
using Extensions.Unity.ImageLoader;
using UnityEngine;
using UnityEngine.UI;

public class SampleErrorHandle : MonoBehaviour
{
    [SerializeField] string imageURL;
    [SerializeField] Image image;

    void Start()
    {
        ImageLoader.LoadSprite(imageURL) // load sprite
            .ThenSet(image) // if success set sprite into image
            .Failed(exception => Debug.LogException(exception)) // if fail print exception
            .Forget();

        ImageLoader.LoadSprite(imageURL) // load sprite
            .ThenSet(image) // if success set sprite into image
            .Then(sprite => image.gameObject.SetActive(true)) // if success activate gameObject
            .Failed(exception => image.gameObject.SetActive(false)) // if fail deactivate gameObject
            .Forget();
    }
}
```

## Sample - Cancellation

``` C#
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
            .ThenSet(image) // if success set sprite into image
            .CancelOnDestroy(this) // cancel OnDestroy event of current gameObject
            .Forget();

        ImageLoader.LoadSprite(imageURL) // load sprite
            .ThenSet(image) // if success set sprite into image
            .Failed(exception => Debug.LogException(exception)) // if fail print exception
            .CancelOnDestroy(this) // cancel OnDestroy event of current gameObject
            .Forget();

        ImageLoader.LoadSprite(imageURL) // load sprite
            .ThenSet(image) // if success set sprite into image
            .Then(sprite => image.gameObject.SetActive(true)) // if success activate gameObject
            .Failed(exception => image.gameObject.SetActive(false)) // if fail deactivate gameObject
            .Canceled(() => Debug.Log("ImageLoading canceled")) // if canceled
            .CancelOnDisable(this) // cancel OnDisable event of current gameObject
            .Forget();
    }

    void SimpleCancellation()
    {
        var future = ImageLoader.LoadSprite(imageURL).ThenSet(image);
        future.Cancel();
    }

    void CancellationTokenSample1()
    {
        var cancellationTokenSource = new CancellationTokenSource();

        // loading with attached cancellation token
        ImageLoader.LoadSprite(imageURL, cancellationToken: cancellationTokenSource.Token)
            .ThenSet(image)
            .Forget();

        cancellationTokenSource.Cancel(); // canceling
    }

    void CancellationTokenSample2()
    {
        var cancellationTokenSource = new CancellationTokenSource();

        ImageLoader.LoadSprite(imageURL)
            .ThenSet(image)
            .Register(cancellationTokenSource.Token) // registering cancellation token
            .Forget();

        cancellationTokenSource.Cancel(); // canceling
    }

    void DisposeSample()
    {
        using (var future = ImageLoader.LoadSprite(imageURL).ThenSet(image))
        {
            // future would be canceled and disposed outside of the brackets
        }
    }
}
```

# Sample - Await and Forget

``` C#
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
```

# Sample - Lifecycle

``` C#
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
            .Canceled(() => Debug.Log("Canceled")) // if canceled
            .Disposed(future => Debug.Log("Disposed")) // if disposed
            .Forget();
    }
}
```

# Texture Memory Management

ImageLoader can manager memory usage of loaded textures. To use it need to call `ImageLoader.LoadSpriteRef` instead of `ImageLoader.LoadSprite`. It will return `Reference<Sprite>` object which contains `Sprite` and `Url` objects. When `Reference<Sprite>` object is not needed anymore, call `Dispose` method to release memory, or just don't save the reference on it. It is `IDisposable` and it will clean itself automatically. Each new instance of `Reference<Sprite>` increments reference counter of the texture. When the last reference is disposed, the texture will be unloaded from memory. Also the all related References will be automatically disposed if you call `ImageLoader.ClearMemoryCache` or `ImageLoader.ClearCache`.

``` C#
// Load sprite image and get reference to it
await ImageLoader.LoadSpriteRef(imageURL);

// Take from Memory cache reference for specific image if exists
ImageLoader.LoadFromMemoryCacheRef(url);
```

# Cache

Cache system based on the two layers. The first layer is **memory cache**, second is **disk cache**. Each layer could be enabled or disabled. Could be used without caching at all. By default both layers are enabled.

## Setup Cache

- `ImageLoader.settings.useMemoryCache = true;` default value is `true`
- `ImageLoader.settings.useDiskCache = true;` default value is `true`

Change disk cache folder:

``` C#
ImageLoader.settings.diskSaveLocation = Application.persistentDataPath + "/myCustomFolder";
```

## Override Cache

``` C#
// Override Memory cache for specific image
ImageLoader.SaveToMemoryCache(url, sprite);

// Take from Memory cache for specific image if exists
ImageLoader.LoadFromMemoryCache(url);
```

## Does Cache contain image

``` C#
// Check if any cache contains specific image
ImageLoader.CacheContains(url);

// Check if Memory cache contains specific image
ImageLoader.MemoryCacheContains(url);

// Check if Memory cache contains specific image
ImageLoader.DiskCacheContains(url);
```

## Clear Cache

``` C#
// Clear memory Memory and Disk cache
ImageLoader.ClearCache();

// Clear only Memory cache for all images
ImageLoader.ClearMemoryCache();

// Clear only Memory cache for specific image
ImageLoader.ClearMemoryCache(url);

// Clear only Disk cache for all images
ImageLoader.ClearDiskCache();

// Clear only Disk cache for specific image
ImageLoader.ClearDiskCache(url);
```
