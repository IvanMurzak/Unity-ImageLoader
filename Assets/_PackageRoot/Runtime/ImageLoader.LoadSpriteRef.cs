using System;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System.Threading;

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
        public static Future<Reference<Sprite>> LoadSpriteRef(string url, TextureFormat textureFormat = TextureFormat.ARGB32, bool ignoreImageNotFoundError = false, CancellationToken cancellationToken = default)
            => LoadSpriteRef(url, new Vector2(0.5f, 0.5f), textureFormat, ignoreImageNotFoundError, cancellationToken);

        /// <summary>
        /// Load image from web or local path and return it as Sprite
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="pivot">Pivot of created Sprite</param>
        /// <param name="textureFormat">TextureFormat for the Texture2D creation</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the image was not found by specified url</param>
        /// <returns>Returns sprite asynchronously </returns>
        public static Future<Reference<Sprite>> LoadSpriteRef(string url, Vector2 pivot, TextureFormat textureFormat = TextureFormat.ARGB32, bool ignoreImageNotFoundError = false, CancellationToken cancellationToken = default)
        {
            var future = new Future<Reference<Sprite>>(url, cancellationToken);
            InternalLoadSpriteRef(future, pivot, textureFormat, ignoreImageNotFoundError);
            return future;
        }
        static async void InternalLoadSpriteRef(Future<Reference<Sprite>> future, Vector2 pivot, TextureFormat textureFormat = TextureFormat.ARGB32, bool ignoreImageNotFoundError = false)
        {
            if (string.IsNullOrEmpty(future.Url))
            {
                if (settings.debugLevel <= DebugLevel.Error)
                    future.FailToLoad(new Exception($"[ImageLoader] Empty url. Image could not be loaded!"));
                return;
            }

            if (MemoryCacheContains(future.Url))
            {
                var reference = LoadFromMemoryCacheRef(future.Url);
                if (reference != null)
                {
                    future.Loaded(reference, FutureLoadedFrom.MemoryCache);
                    return;
                }
            }

            if (IsLoading(future.Url))
            {
                if (settings.debugLevel <= DebugLevel.Log)
                    Debug.Log($"[ImageLoader] Waiting while another task is loading the sprite url={future.Url}");
                await UniTask.WaitWhile(() => IsLoading(future.Url));
                if (future.IsCancelled) return;
                InternalLoadSpriteRef(future, pivot, textureFormat, ignoreImageNotFoundError);
                return;
            }

            AddLoading(future.Url);

            if (settings.debugLevel <= DebugLevel.Log)
                Debug.Log($"[ImageLoader] Loading new Sprite into memory url={future.Url}");
            try
            {
                var cachedImage = await LoadDiskAsync(future.Url);
                if (cachedImage != null && cachedImage.Length > 0)
                {
                    await UniTask.SwitchToMainThread();
                    if (future.IsCancelled) return;
                    var texture = new Texture2D(2, 2, textureFormat, true);
                    if (texture.LoadImage(cachedImage))
                    {
                        var sprite = ToSprite(texture, pivot);
                        if (sprite != null)
                            SaveToMemoryCache(future.Url, sprite, replace: true);

                        RemoveLoading(future.Url);
                        future.Loaded(new Reference<Sprite>(future.Url, sprite), FutureLoadedFrom.DiskCache);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                future.Cancel();
            }
            catch (Exception e)
            {
                if (settings.debugLevel <= DebugLevel.Exception)
                    Debug.LogException(e);
            }

            UnityWebRequest request = null;
            var finished = false;
            try
            {
                UniTask.Post(async () =>
                {
                    try
                    {
                        request = UnityWebRequestTexture.GetTexture(future.Url);
                        await request.SendWebRequest();
                    }
                    catch (OperationCanceledException)
                    {
                        future.Cancel();
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
                if (future.IsCancelled) return;
#if UNITY_2020_1_OR_NEWER
                var isError = request.result != UnityWebRequest.Result.Success;
#else
                var isError = request.isNetworkError || request.isHttpError;
#endif
                if (isError)
                {
#if UNITY_2020_1_OR_NEWER
                    var exception = new Exception($"[ImageLoader] {request.result} {request.error}: url={future.Url}");
#else
                    var exception = new Exception($"[ImageLoader] {request.error}: url={future.Url}");
#endif
                    future.FailToLoad(exception);
                }
                else
                {
                    await SaveDiskAsync(future.Url, request.downloadHandler.data);
                    if (future.IsCancelled) return;
                    var sprite = ToSprite(((DownloadHandlerTexture)request.downloadHandler).texture);
                    SaveToMemoryCache(future.Url, sprite, replace: true);
                    future.Loaded(new Reference<Sprite>(future.Url, sprite), FutureLoadedFrom.Source);
                }
            }
            catch (OperationCanceledException)
            {
                future.Cancel();
            }
            finally
            {
                RemoveLoading(future.Url);
            }
        }
    }
}