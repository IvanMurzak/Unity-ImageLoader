using System;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
        private static HashSet<string> loadingInProcess = new HashSet<string>();
        private static void AddLoading(string url) => loadingInProcess.Add(url);
        private static void RemoveLoading(string url) => loadingInProcess.Remove(url);

        /// <summary>
        /// Initialization of static variables, should be called from main thread at project start
        /// </summary>
        public static void Init()
        {
            // need get SaveLocation variable in runtime from thread to setup the default static value into it
            var temp = settings.diskSaveLocation + settings.diskSaveLocation;
        }

        /// <summary>
        /// Check if the url is loading right now
        /// </summary>
        /// <returns>Returns true if the url is loading right now</returns>
        public static bool IsLoading(string url) => loadingInProcess.Contains(url);

        /// <summary>
        /// Clear cache from Memory and Disk layers for all urls
        /// </summary>
        public static Task ClearCache()
        {
            ClearMemoryCache();
            return ClearDiskCache();
        }

        /// <summary>
        /// Checks cache at Memory and at Disk by the url
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns true if cache exists</returns>
        public static bool CacheContains(string url) => MemoryCacheContains(url) || DiskCacheContains(url);

        /// <summary>
        /// Converts Texture2D to Sprite
        /// </summary>
        /// <param name="texture">Texture for creation Sprite</param>
        /// <param name="pixelDensity">Pixel density of the Sprite</param>
        /// <returns>Returns sprite</returns>
        public static Sprite ToSprite(Texture2D texture, float pixelDensity = 100f) 
            => Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelDensity);

        /// <summary>
        /// Converts Texture2D to Sprite
        /// </summary>
        /// <param name="texture">Texture for creation Sprite</param>
        /// <param name="pivot">Pivot of created Sprite</param>
        /// <param name="pixelDensity">Pixel density of the Sprite</param>
        /// <returns>Returns sprite</returns>
        public static Sprite ToSprite(Texture2D texture, Vector2 pivot, float pixelDensity = 100f) 
            => Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), pivot, pixelDensity);

        /// <summary>
        /// Load image from web or local path and return it as Sprite
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="textureFormat">TextureFormat for the Texture2D creation</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the image was not found by specified url</param>
        /// <returns>Returns sprite asynchronously </returns>
        public static UniTask<Sprite> LoadSprite(string url, TextureFormat textureFormat = TextureFormat.ARGB32, bool ignoreImageNotFoundError = false)
            => LoadSprite(url, Vector2.one * 0.5f, textureFormat, ignoreImageNotFoundError);

        /// <summary>
        /// Load image from web or local path and return it as Sprite
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="pivot">Pivot of created Sprite</param>
        /// <param name="textureFormat">TextureFormat for the Texture2D creation</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the image was not found by specified url</param>
        /// <returns>Returns sprite asynchronously </returns>
        public static async UniTask<Sprite> LoadSprite(string url, Vector2 pivot, TextureFormat textureFormat = TextureFormat.ARGB32, bool ignoreImageNotFoundError = false)
        {
            if (string.IsNullOrEmpty(url))
            {
                if (settings.debugLevel <= DebugLevel.Error)
                    Debug.LogError($"[ImageLoader] Empty url. Image could not be loaded!");
                return null;
            }

            if (MemoryCacheContains(url))
            {
                var sprite = LoadFromMemoryCache(url);
                if (sprite != null)
                    return sprite;
            }

            if (IsLoading(url))
            {
                if (settings.debugLevel <= DebugLevel.Log)
                    Debug.Log($"[ImageLoader] Waiting while another task is loading the sprite url={url}");
                await UniTask.WaitWhile(() => IsLoading(url));
                return await LoadSprite(url, textureFormat, ignoreImageNotFoundError);
            }

            AddLoading(url);

            if (settings.debugLevel <= DebugLevel.Log)
                Debug.Log($"[ImageLoader] Loading new Sprite into memory url={url}");
            try
            {
                var cachedImage = await LoadDiskAsync(url);
                if (cachedImage != null && cachedImage.Length > 0)
                {
                    await UniTask.SwitchToMainThread();
                    var texture = new Texture2D(2, 2, textureFormat, true);
                    if (texture.LoadImage(cachedImage))
                    {
                        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot);
                        if (sprite != null)
                            SaveToMemoryCache(url, sprite, replace: true);

                        RemoveLoading(url);
                        return sprite;
                    }
                }
            }
            catch (Exception e)
            {
                if (settings.debugLevel <= DebugLevel.Exception)
                    Debug.LogException(e);
            }

            UnityWebRequest request = null;
            var finished = false;
            UniTask.Post(async () =>
            {
                try
                {
                    request = UnityWebRequestTexture.GetTexture(url);
                    await request.SendWebRequest();
                }
                catch (Exception e) 
                {
                    if (!ignoreImageNotFoundError)
                        if (settings.debugLevel <= DebugLevel.Exception)
                            Debug.LogException(e);
                }
                finally
                {
                    finished = true;
                }
            });
            await UniTask.WaitUntil(() => finished);

            RemoveLoading(url);

            if (request.isNetworkError || request.isHttpError)
            {
                if (settings.debugLevel <= DebugLevel.Error)
                    Debug.LogError($"[ImageLoader] {request.error}: url={url}");
                return null;
            }
            else
            {
                await SaveDiskAsync(url, request.downloadHandler.data);
                var sprite = ToSprite(((DownloadHandlerTexture)request.downloadHandler).texture);
                SaveToMemoryCache(url, sprite, replace: true);
                return sprite;
            }
        }
    }
}