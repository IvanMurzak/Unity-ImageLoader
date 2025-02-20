# Unity Image Loader

![npm](https://img.shields.io/npm/v/extensions.unity.imageloader) [![openupm](https://img.shields.io/npm/v/extensions.unity.imageloader?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/extensions.unity.imageloader/) ![License](https://img.shields.io/github/license/IvanMurzak/Unity-ImageLoader) [![Stand With Ukraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/badges/StandWithUkraine.svg)](https://stand-with-ukraine.pp.ua)
![2019.4.40f1](https://img.shields.io/github/actions/workflow/status/IvanMurzak/Unity-ImageLoader/2019.4.40f1_editor.yml?label=2019.4.40f1-Editor) ![2020.3.40f1](https://img.shields.io/github/actions/workflow/status/IvanMurzak/Unity-ImageLoader/2020.3.40f1_editor.yml?label=2020.3.40f1-Editor) ![2021.3.45f1](https://img.shields.io/github/actions/workflow/status/IvanMurzak/Unity-ImageLoader/2021.3.45f1_editor.yml?label=2021.3.45f1-Editor) ![2022.3.57f1](https://img.shields.io/github/actions/workflow/status/IvanMurzak/Unity-ImageLoader/2022.3.57f1_editor.yml?label=2022.3.57f1-Editor) ![2023.1.20f1](https://img.shields.io/github/actions/workflow/status/IvanMurzak/Unity-ImageLoader/2023.1.20f1_editor.yml?label=2023.1.20f1-Editor) ![2023.2.20f1](https://img.shields.io/github/actions/workflow/status/IvanMurzak/Unity-ImageLoader/2023.2.20f1_editor.yml?label=2023.2.20f1-Editor) ![6000.0.37f1](https://img.shields.io/github/actions/workflow/status/IvanMurzak/Unity-ImageLoader/6000.0.37f1_editor.yml?label=6000.0.37f1-Editor)

Async image loader with two caching layers for Unity. It supports loading images from web or local paths and provides memory and disk caching to optimize performance. The package includes features for automatic image setting, cancellation handling, error handling, and lifecycle management.

Wait for image get loaded then set:

``` C#
image.sprite = await ImageLoader.LoadSprite(imageURL);
```

Don't wait, use callback to set loaded image later:

``` C#
ImageLoader.LoadSprite(imageURL).ThenSet(image).Forget();
```

Use callback to set image and still wait for the completion:

``` C#
await ImageLoader.LoadSprite(imageURL).ThenSet(image);
```

## Features

- ✔️ Async loading from **Web** or **Local** `ImageLoader.LoadSprite(imageURL);`
- ✔️ **Memory** and **Disk** caching - tries to load from memory first, then from disk
- ✔️ Dedicated thread for disk operations
- ✔️ Supports loading of `Texture2D` and `Sprite`
- ✔️ Avoids loading same image multiple times simultaneously, a new load task waits for completion of existed task
- ✔️ Uses `UnityWebRequest` to load data which works smooth across all platforms including `WebGL`
- ✔️ Cache supported on at `WebGL`. Memory cache works, Disk cache isn't allowed by the platform
- ✔️ Set into Image `ImageLoader.LoadSprite(imageURL).ThenSet(image);`
- ✔️ Set into RawImage `ImageLoader.LoadSprite(imageURL).ThenSet(rawImage);`
- ✔️ Set into Material `ImageLoader.LoadSprite(imageURL).ThenSet("_MainTex", material);`
- ✔️ Set into SpriteRenderer `ImageLoader.LoadSprite(imageURL).ThenSet(spriteRenderer);`
- ✔️ [Set into anything](#cancellation)
- ✔️ Cancellation `ImageLoader.LoadSprite(imageURL).Cancel();`
- ✔️ Cancellation callback `ImageLoader.LoadSprite(imageURL).Cancelled(() => ...);`
- ✔️ Error callback `ImageLoader.LoadSprite(imageURL).Failed(exception => ...);`
- ✔️ Debug level for logging `ImageLoader.settings.debugLevel = DebugLevel.Error;`
- ✔️ Debug level per each task `ImageLoader.LoadSprite(imageURL).SetLogLevel(DebugLevel.Trace);`

## Content

- [Unity Image Loader](#unity-image-loader)
  - [Features](#features)
  - [Content](#content)
  - [Installation](#installation)
- [Usage](#usage)
  - [Events lifecycle](#events-lifecycle)
  - [Load `Sprite` then set into `Image`](#load-sprite-then-set-into-image)
  - [Load `Texture2D` then set into `Material`](#load-texture2d-then-set-into-material)
  - [Load `Sprite` then set into multiple `Image`](#load-sprite-then-set-into-multiple-image)
  - [Error handling](#error-handling)
  - [Async `await` and `Forget`](#async-await-and-forget)
  - [Cancellation](#cancellation)
    - [Cancel by MonoBehaviour events](#cancel-by-monobehaviour-events)
    - [Explicit cancellation](#explicit-cancellation)
    - [Cancellation Token](#cancellation-token)
    - [Cancellation by `using`](#cancellation-by-using)
    - [Timeout](#timeout)
  - [Cache](#cache)
    - [Setup Cache](#setup-cache)
      - [Change Disk cache folder](#change-disk-cache-folder)
    - [Manually read / write into cache](#manually-read--write-into-cache)
    - [Check cache existence](#check-cache-existence)
    - [Clear cache](#clear-cache)
  - [Texture Memory Management](#texture-memory-management)
    - [\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_](#____________________)
    - [\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_](#____________________-1)
    - [\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_](#____________________-2)
  - [Other](#other)
    - [Understanding `IFuture<T>`](#understanding-ifuturet)
    - [Understanding `Reference<T>`](#understanding-referencet)
    - [Understanding `Future<Reference<T>>`](#understanding-futurereferencet)

---

## Installation

- [Install OpenUPM-CLI](https://github.com/openupm/openupm-cli#installation)
- Open command line in Unity project folder
- Run the command

``` CLI
openupm add extensions.unity.imageloader
```

# Usage

In the main thread somewhere at the start of the project need to call `ImageLoader.Init();` once to initialize static properties in the right thread. It is required to make in the main thread. Then you can use `ImageLoader` from any thread and at any time.

``` C#
ImageLoader.Init(); // just once from the main thread
```

## Events lifecycle

> [Full sample source code](https://github.com/IvanMurzak/Unity-ImageLoader/blob/master/Assets/_PackageRoot/Samples/SampleLifecycle.cs)

`ImageLoader.LoadSprite` returns `IFuture<Sprite>`. This instance provides all range of callbacks and API to modify it. Read more about [Future](#future).

``` C#
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
```

## Load `Sprite` then set into `Image`

> [Full sample source code](https://github.com/IvanMurzak/Unity-ImageLoader/blob/master/Assets/_PackageRoot/Samples/SampleLoadSpriteThenSetImage.cs)

``` C#
// Load a sprite from the web and cache it for faster loading next time
image.sprite = await ImageLoader.LoadSprite(imageURL);

// Load a sprite from the web and set it directly to the Image component
await ImageLoader.LoadSprite(imageURL).ThenSet(image);
```

## Load `Texture2D` then set into `Material`

> [Full sample source code](https://github.com/IvanMurzak/Unity-ImageLoader/blob/master/Assets/_PackageRoot/Samples/SampleLoadTextureThenSetMaterial.cs)

``` C#
// Load a Texture2D from the web and cache it for faster loading next time
material.mainTexture = await ImageLoader.LoadTexture(imageURL);

// Load a Texture2D from the web and set it directly to the Material
await ImageLoader.LoadTexture(imageURL).ThenSet(material);
```

## Load `Sprite` then set into multiple `Image`

> [Full sample source code](https://github.com/IvanMurzak/Unity-ImageLoader/blob/master/Assets/_PackageRoot/Samples/SampleLoadSpriteThenSetIntoMultipleImages.cs)

``` C#
ImageLoader.LoadSprite(imageURL).ThenSet(image1, image2).Forget();
```

## Error handling

> [Full sample source code](https://github.com/IvanMurzak/Unity-ImageLoader/blob/master/Assets/_PackageRoot/Samples/SampleErrorHandle.cs)

``` C#
ImageLoader.LoadSprite(imageURL) // Attempt to load a sprite
    .ThenSet(image) // If successful, set the sprite to the Image component
    .Failed(exception => Debug.LogException(exception)) // If an error occurs, log the exception
    .Forget(); // Forget the task to avoid compilation warning

ImageLoader.LoadSprite(imageURL) // Attempt to load a sprite
    .ThenSet(image) // If successful, set the sprite to the Image component
    .Then(sprite => image.gameObject.SetActive(true)) // If successful, activate the GameObject
    .Failed(exception => image.gameObject.SetActive(false)) // If an error occurs, deactivate the GameObject
    .Forget(); // Forget the task to avoid compilation warning
```

## Async `await` and `Forget`

> [Full sample source code](https://github.com/IvanMurzak/Unity-ImageLoader/blob/master/Assets/_PackageRoot/Samples/SampleAwaitAndForget.cs)

``` C#
// Load image and wait
await ImageLoader.LoadSprite(imageURL);

// Load image, set image and wait
await ImageLoader.LoadSprite(imageURL).ThenSet(image);

// Skip waiting for completion.
// To do that we can simply remove 'await' from the start.
// To avoid compilation warning need to add '.Forget()'.
ImageLoader.LoadSprite(imageURL).ThenSet(image).Forget();
```

## Cancellation

> [Full sample source code](https://github.com/IvanMurzak/Unity-ImageLoader/blob/master/Assets/_PackageRoot/Samples/SampleCancellation.cs)

### Cancel by MonoBehaviour events

``` C#
ImageLoader.LoadSprite(imageURL)
    .ThenSet(image)
    .CancelOnEnable(this)   // cancel on OnEnable event of current MonoBehaviour
    .CancelOnDisable(this)  // cancel on OnDisable event of current MonoBehaviour
    .CancelOnDestroy(this); // cancel on OnDestroy event of current MonoBehaviour
```

### Explicit cancellation

``` C#
var future = ImageLoader.LoadSprite(imageURL).ThenSet(image);
future.Cancel();
```

### Cancellation Token

``` C#
var cancellationTokenSource = new CancellationTokenSource();

// loading with attached cancellation token
ImageLoader.LoadSprite(imageURL, cancellationToken: cancellationTokenSource.Token)
    .ThenSet(image)
    .Forget();

cancellationTokenSource.Cancel(); // canceling
```

``` C#
var cancellationTokenSource = new CancellationTokenSource();

ImageLoader.LoadSprite(imageURL)
    .ThenSet(image)
    .Register(cancellationTokenSource.Token) // registering cancellation token
    .Forget();

cancellationTokenSource.Cancel(); // canceling
```

### Cancellation by `using`

``` C#
using (var future = ImageLoader.LoadSprite(imageURL).ThenSet(image))
{
    // future would be canceled and disposed outside of the brackets
}
```

``` C#
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
    .Canceled(() => Debug.Log("ImageLoading canceled")) // if cancelled
    .CancelOnDisable(this) // cancel OnDisable event of current gameObject
    .Forget();
```

### Timeout

> [Full sample source code](https://github.com/IvanMurzak/Unity-ImageLoader/blob/master/Assets/_PackageRoot/Samples/SampleTimeout.cs)

``` C#
ImageLoader.LoadSprite(imageURL) // load sprite
    .ThenSet(image) // if success set sprite into image
    .Timeout(TimeSpan.FromSeconds(10)) // set timeout duration 10 seconds
    .Forget();
```

## Cache

Cache system based on the two layers. The first layer is **Memory cache**, second is **Disk cache**. Each layer could be enabled or disabled. Could be used without caching at all. By default both layers are enabled. `WebGL` doesn't support Disk cache, because it doesn't have access to disk.

### Setup Cache

- `ImageLoader.settings.useMemoryCache = true;` default value is `true`
- `ImageLoader.settings.useDiskCache = true;` default value is `true`

#### Change Disk cache folder

By default it uses `Application.persistentDataPath + "/ImageLoader"`

``` C#
ImageLoader.settings.diskSaveLocation = Application.persistentDataPath + "/myCustomFolder";
```

> [Full sample source code](https://github.com/IvanMurzak/Unity-ImageLoader/blob/master/Assets/_PackageRoot/Samples/SampleCache.cs)

### Manually read / write into cache

``` C#
// Override Memory cache for specific image
ImageLoader.SaveToMemoryCache(url, sprite);

// Take from Memory cache for specific image if exists
ImageLoader.LoadSpriteFromMemoryCache(url);
```

### Check cache existence

``` C#
// Check if any cache contains specific image
ImageLoader.CacheContains(url);

// Check if Memory cache contains specific image
ImageLoader.MemoryCacheContains(url);

// Check if Disk cache contains specific image
ImageLoader.DiskCacheContains(url);
```

### Clear cache

``` C#
// Clear memory Memory and Disk cache
ImageLoader.ClearCacheAll();

// Clear only Memory cache for all images
ImageLoader.ClearMemoryCacheAll();

// Clear only Memory cache for specific image
ImageLoader.ClearMemoryCache(url);

// Clear only Disk cache for all images
ImageLoader.ClearDiskCacheAll();

// Clear only Disk cache for specific image
ImageLoader.ClearDiskCache(url);
```

Memory cache could be cleared automatically if to use `Reference<T>` and the heavy `Texture2D` memory would be released as well. Read more at [Texture Memory Management](#texture-memory-management).

## Texture Memory Management

ImageLoader can manager memory usage of loaded textures. To use it need to call `ImageLoader.LoadSpriteRef` instead of `ImageLoader.LoadSprite`. It will return `Reference<Sprite>` object which contains `Sprite` and `Url` objects. When `Reference<Sprite>` object is not needed anymore, call `Dispose` method to release memory, or just don't save the reference on it. It is `IDisposable` and it will clean itself automatically. Each new instance of `Reference<Sprite>` increments reference counter of the texture. When the last reference is disposed, the texture will be unloaded from memory. Also the all related References will be automatically disposed if you call `ImageLoader.ClearMemoryCache` or `ImageLoader.ClearCache`.

``` C#
// Load sprite image and get reference to it
await ImageLoader.LoadSpriteRef(imageURL);

// Take from Memory cache reference for specific image if exists
ImageLoader.LoadSpriteRefFromMemoryCache(url);
```

> :warning: Releasing `Texture2D` from memory while any Unity's component uses it may trigger native app crash or even Unity Editor crash. Please pay enough attention to manage `Reference<T>` instances in a proper way. Or do not use them.

### ____________________

> [Full sample source code](https://github.com/IvanMurzak/Unity-ImageLoader/blob/master/Assets/_PackageRoot/Samples/SampleReferences.cs)

``` C#
____________________________
```

### ____________________

> [Full sample source code](https://github.com/IvanMurzak/Unity-ImageLoader/blob/master/Assets/_PackageRoot/Samples/______________________.cs)

``` C#
____________________________
```

### ____________________

> [Full sample source code](https://github.com/IvanMurzak/Unity-ImageLoader/blob/master/Assets/_PackageRoot/Samples/______________________.cs)

``` C#
____________________________
```

## Other

### Understanding `IFuture<T>`

### Understanding `Reference<T>`

### Understanding `Future<Reference<T>>`
