using System;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
        /// <summary>
        /// Load image from web or local path and return it as Sprite
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="textureFormat">TextureFormat for the Texture2D creation</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the image was not found by specified url</param>
        /// <returns>Returns sprite asynchronously </returns>
        public static UniTask<Reference<Sprite>> LoadSpriteRef(string url, TextureFormat textureFormat = TextureFormat.ARGB32, bool ignoreImageNotFoundError = false)
            => LoadSpriteRef(url, new Vector2(0.5f, 0.5f), textureFormat, ignoreImageNotFoundError);

        /// <summary>
        /// Load image from web or local path and return it as Sprite
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="pivot">Pivot of created Sprite</param>
        /// <param name="textureFormat">TextureFormat for the Texture2D creation</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the image was not found by specified url</param>
        /// <returns>Returns sprite asynchronously </returns>
        public static async UniTask<Reference<Sprite>> LoadSpriteRef(string url, Vector2 pivot, TextureFormat textureFormat = TextureFormat.ARGB32, bool ignoreImageNotFoundError = false)
        {
            if (string.IsNullOrEmpty(url))
            {
                if (settings.debugLevel <= DebugLevel.Error)
                    Debug.LogError($"[ImageLoader] Empty url. Image could not be loaded!");
                return null;
            }

            if (MemoryCacheContains(url))
            {
                var reference = LoadFromMemoryCacheRef(url);
                if (reference != null)
                    return reference;
            }

            if (IsLoading(url))
            {
                if (settings.debugLevel <= DebugLevel.Log)
                    Debug.Log($"[ImageLoader] Waiting while another task is loading the sprite url={url}");
                await UniTask.WaitWhile(() => IsLoading(url));
                return await LoadSpriteRef(url, textureFormat, ignoreImageNotFoundError);
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
                        var sprite = ToSprite(texture, pivot);
                        if (sprite != null)
                            SaveToMemoryCache(url, sprite, replace: true);

                        RemoveLoading(url);
                        return new Reference<Sprite>(url, sprite);
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

#if UNITY_2020_1_OR_NEWER
            var isError = request.result != UnityWebRequest.Result.Success;
#else
            var isError = request.isNetworkError || request.isHttpError;
#endif

            if (isError)
            {
                if (settings.debugLevel <= DebugLevel.Error)
#if UNITY_2020_1_OR_NEWER
                    Debug.LogError($"[ImageLoader] {request.result} {request.error}: url={url}");
#else
                    Debug.LogError($"[ImageLoader] {request.error}: url={url}");
#endif
                return null;
            }
            else
            {
                await SaveDiskAsync(url, request.downloadHandler.data);
                var sprite = ToSprite(((DownloadHandlerTexture)request.downloadHandler).texture);
                SaveToMemoryCache(url, sprite, replace: true);
                return new Reference<Sprite>(url, sprite);
            }
        }
    }
}