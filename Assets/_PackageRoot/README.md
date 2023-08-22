# Unity Image Loader

![npm](https://img.shields.io/npm/v/extensions.unity.imageloader) [![openupm](https://img.shields.io/npm/v/extensions.unity.imageloader?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/extensions.unity.imageloader/) ![License](https://img.shields.io/github/license/IvanMurzak/Unity-ImageLoader) [![Stand With Ukraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/badges/StandWithUkraine.svg)](https://stand-with-ukraine.pp.ua)

Async image loader with two caching layers for Unity.

## Features

- ✔️ Async loading from **Web** or **Local** `ImageLoader.LoadSprite(imageURL);`
- ✔️ **Memory** and **Disk** caching - tries to load from memory first, then from disk
- ✔️ Dedicated thread for disk operations
- ✔️ Avoids loading same image multiple times simultaneously, task waits for completion the first and just returns loaded image if at least one cache layer activated
- ✔️ Auto set to Image `ImageLoader.SetSprite(imageURL, image);`
- ✔️ Auto set to SpriteRenderer `ImageLoader.SetSprite(imageURL, spriteRenderer);`
- ✔️ Debug level for logging `ImageLoader.settings.debugLevel = DebugLevel.Error;`

# Usage

In the main thread somewhere at the start of the project need to call `ImageLoader.Init();` once to initialize static properties in the right thread. It is required to make in the main thread. Then you can use `ImageLoader` from any thread and at any time.

## Sample - Loading Sprite, set to Image

``` C#
using Extensions.Unity.ImageLoader;
using Cysharp.Threading.Tasks;

public class ImageLoaderSample : MonoBehaviour
{
    [SerializeField] string imageURL;
    [SerializeField] Image image;

    async void Start()
    {
        // Loading sprite from web, cached for quick load next time
        image.sprite = await ImageLoader.LoadSprite(imageURL);

        // Same loading with auto set to image
        await ImageLoader.SetSprite(imageURL, image);
    }
}
```

## Sample - Loading image into multiple Image components

``` C#
using Extensions.Unity.ImageLoader;
using Cysharp.Threading.Tasks;

public class ImageLoaderSample : MonoBehaviour
{
    [SerializeField] string imageURL;
    [SerializeField] Image image1;
    [SerializeField] Image image2;

    void Start()
    {
        // Loading with auto set to image
        ImageLoader.SetSprite(imageURL, image1, image2).Forget();
    }
}
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

# Installation

- [Install OpenUPM-CLI](https://github.com/openupm/openupm-cli#installation)
- Open command line in Unity project folder
- Run the command

``` CLI
openupm add extensions.unity.imageloader
```
