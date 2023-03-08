using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
        public static bool MemoryCacheExists(string url) => loadedSpritesCache.ContainsKey(url);
        public static bool DiskCacheExists(string url) => File.Exists(DiskCachePath(url));

        public static string SaveLocation { get; set; } = Application.persistentDataPath + "/imageCache";

        public static void Init()
        {
            // need get SaveLocation variable in runtime from thread to setup the default static value into it
            var temp = SaveLocation + SaveLocation;
        }

        public static void ClearDiskCache()
        {
            if (Directory.Exists(SaveLocation))
                Directory.Delete(SaveLocation, true);
        }
        public static void ClearMemoryCache()
        {
            foreach (var cache in loadedSpritesCache.Values)
            {
                if (cache?.texture != null)
                    UnityEngine.Object.DestroyImmediate(cache.texture);
            }
            loadedSpritesCache.Clear();
        }
        public static void ClearCache()
        {
            ClearMemoryCache();
            ClearDiskCache();
        }

        public static Sprite ToSprite(Texture2D texture, float pixelDensity = 100f) 
            => Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelDensity);
        public static Sprite ToSprite(Texture2D texture, Vector2 pivot, float pixelDensity = 100f) 
            => Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), pivot, pixelDensity);

        public static async UniTask<Sprite> LoadSprite(string url, TextureFormat textureFormat = TextureFormat.ARGB32, bool ignoreImageNotFoundError = false)
        {
            if (string.IsNullOrEmpty(url))
            {
                if (settings.debugMode <= DebugMode.Error)
                    Debug.LogError($"ImageLoader: Empty url. Image could not be loaded!");
                return null;
            }

            if (loadedSpritesCache.ContainsKey(url))
            {
                var sprite = loadedSpritesCache[url];
                if (sprite != null)
                    return sprite;
            }

            if (loadingInProcess.Contains(url))
            {
                if (settings.debugMode <= DebugMode.Log)
                    Debug.Log($"ImageLoader: Waiting while another task is loading the sprite url={url}");
                await UniTask.WaitWhile(() => loadingInProcess.Contains(url));
                return await LoadSprite(url, textureFormat, ignoreImageNotFoundError);
            }
            loadingInProcess.Add(url);

            if (settings.debugMode <= DebugMode.Log)
                Debug.Log($"ImageLoader: Loading new Sprite into memory url={url}");
            try
            {
                var cachedImage = await LoadAsync(url);
                if (cachedImage != null && cachedImage.Length > 0)
                {
                    await UniTask.SwitchToMainThread();
                    var texture = new Texture2D(2, 2, textureFormat, true);
                    if (texture.LoadImage(cachedImage))
                    {
                        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f));
                        if (sprite != null)
                            loadedSpritesCache[url] = sprite;

                        return sprite;
                    }
                }
            }
            catch (Exception e)
            {
                if (settings.debugMode <= DebugMode.Exception)
                    Debug.LogException(e);
            }
            finally
            {
                loadingInProcess.Remove(url);
            }

            UnityWebRequest request = null;
            var finished = false;
            UniTask.Post(async () =>
            {
                if (ignoreImageNotFoundError)
                {
                    try
                    {
                        request = UnityWebRequestTexture.GetTexture(url);
                        await request.SendWebRequest();
                    }
                    catch (Exception e) 
                    { 
                        if (settings.debugMode <= DebugMode.Exception)
                            Debug.LogException(e); 
                    }
                    finally
                    {
                        finished = true;
                    }
                }
                else
                {
                    try
                    {
                        request = UnityWebRequestTexture.GetTexture(url);
                        await request.SendWebRequest();
                    }
                    finally
                    {
                        finished = true;
                    }
                }
            });
            await UniTask.WaitUntil(() => finished);
            loadingInProcess.Remove(url);

            if (request.isNetworkError || request.isHttpError)
            {
                if (settings.debugMode <= DebugMode.Error)
                    Debug.LogError($"ImageLoader: {request.error}: url={url}");
                return null;
            }
            else
            {
                await SaveAsync(url, request.downloadHandler.data);
                var sprite = ToSprite(((DownloadHandlerTexture)request.downloadHandler).texture);
                loadedSpritesCache[url] = sprite;
                return sprite;
            }
        }

        public static async UniTask SetImage(string url, Image image)
        {
            try
            {
                if (image == null || image.IsDestroyed() || image.gameObject == null)
                    return;

                var sprite = await LoadSprite(url);
                UniTask.Post(() =>
                {
                    if (image == null || image.IsDestroyed() || GameObject.Equals(image.gameObject, null))
                        return;
                    try
                    {
                        image.sprite = sprite;
                    }
                    catch (Exception e)
                    {
                        if (settings.debugMode <= DebugMode.Exception)
                            Debug.LogException(e); 
                    }
                });
            }
            catch (Exception e) 
            { 
                if (settings.debugMode <= DebugMode.Exception)
                    Debug.LogException(e); 
            }
        }
        public static async UniTask SetImage(string url, params Image[] images)
        {
            if (images == null)
                return;

            var sprite = await LoadSprite(url);
            UniTask.Post(() =>
            {
                for (var i = 0; i < images.Length; i++)
                {
                    try
                    {
                        if (images[i] == null || images[i].IsDestroyed() || GameObject.Equals(images[i].gameObject, null))
                            continue;

                        images[i].sprite = sprite;
                    }
                    catch (Exception e) 
                    {
                        if (settings.debugMode <= DebugMode.Exception)
                            Debug.LogException(e); 
                    }
                }
            });
        }
    }
}