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
        public static Future<Sprite> LoadSprite(string url, TextureFormat textureFormat = TextureFormat.ARGB32, bool ignoreImageNotFoundError = false, CancellationToken cancellationToken = default)
            => LoadSprite(url, new Vector2(0.5f, 0.5f), textureFormat, ignoreImageNotFoundError, cancellationToken);


        /// <summary>
        /// Load image from web or local path and return it as Sprite
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="pivot">Pivot of created Sprite</param>
        /// <param name="textureFormat">TextureFormat for the Texture2D creation</param>
        /// <param name="ignoreImageNotFoundError">Ignore error if the image was not found by specified url</param>
        /// <returns>Returns sprite asynchronously </returns>
        public static Future<Sprite> LoadSprite(string url, Vector2 pivot, TextureFormat textureFormat = TextureFormat.ARGB32, bool ignoreImageNotFoundError = false, CancellationToken cancellationToken = default)
        {
            var future = new Future<Sprite>(url, cancellationToken);
            InternalLoadSprite(future, pivot, textureFormat, ignoreImageNotFoundError);
            return future;
        }
        static async void InternalLoadSprite(Future<Sprite> future, Vector2 pivot, TextureFormat textureFormat = TextureFormat.ARGB32, bool ignoreImageNotFoundError = false)
        {
            if (future.IsCancelled || future.Status == FutureStatus.Disposed)
                return;

            if (string.IsNullOrEmpty(future.Url))
            {
                future.FailToLoad(new Exception($"[ImageLoader] Future[id={future.id}] Empty url. Image could not be loaded!"));
                return;
            }

            if (future.UseMemoryCache && MemoryCacheContains(future.Url))
            {
                var sprite = LoadFromMemoryCache(future.Url);
                if (sprite != null)
                {
                    future.Loaded(sprite, FutureLoadedFrom.MemoryCache);
                    return;
                }
            }

            var anotherLoadingFuture = GetLoadingFuture(future.Url);
            if (anotherLoadingFuture != null)
            {
                if (future.LogLevel.IsActive(DebugLevel.Log))
                    Debug.Log($"[ImageLoader] Future[id={future.id}] Waiting while another task is loading {future.Url}");

                anotherLoadingFuture.PassEvents(future, passCancelled: false).Forget();
                await UniTask.WaitWhile(() => IsLoading(future.Url) && !future.IsCancelled);

                if (future.LogLevel.IsActive(DebugLevel.Log))
                {
                    Debug.Log(future.IsCancelled
                        ? $"[ImageLoader] Future[id={future.id}] Cancelled {future.Url}"
                        : future.Status == FutureStatus.FailedToLoad
                            ? $"[ImageLoader] Future[id={future.id}] Another task. Failed to load {future.Url}"
                            : $"[ImageLoader] Future[id={future.id}] Another task. Complete waiting for another task to load {future.Url}");
                }
                if (future.IsCancelled || future.Status == FutureStatus.FailedToLoad)
                    return;
                InternalLoadSprite(future, pivot, textureFormat, ignoreImageNotFoundError);
                return;
            }

            AddLoading(future); // LOADING ADDED

            if (future.UseDiskCache && DiskCacheContains(future.Url))
            {
                future.Loading(FutureLoadingFrom.DiskCache);
                try
                {
                    var cachedImage = await LoadDiskAsync(future.Url);
                    if (cachedImage != null && cachedImage.Length > 0)
                    {
                        await UniTask.SwitchToMainThread();
                        if (future.IsCancelled || future.Status == FutureStatus.FailedToLoad)
                        {
                            RemoveLoading(future); // LOADING REMOVED
                            return;
                        }
                        var texture = new Texture2D(2, 2, textureFormat, true);
                        if (texture.LoadImage(cachedImage))
                        {
                            var sprite = ToSprite(texture, pivot);
                            if (sprite != null && future.UseMemoryCache)
                                SaveToMemoryCache(future.Url, sprite, replace: true);

                            RemoveLoading(future); // LOADING REMOVED
                            if (future.IsCancelled || future.Status == FutureStatus.FailedToLoad)
                                return;
                            future.Loaded(sprite, FutureLoadedFrom.DiskCache);
                            return;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    RemoveLoading(future); // LOADING REMOVED
                    future.Cancel();
                    return;
                }
                catch (Exception e)
                {
                    if (future.LogLevel.IsActive(DebugLevel.Exception))
                        Debug.LogException(e);
                }
            }

            future.Loading(FutureLoadingFrom.Source);

            var finished = false;
            UniTask.Post(async () =>
            {
                try
                {
                    if (future.IsCancelled || future.Status == FutureStatus.FailedToLoad)
                        return;

                    if (future.LogLevel.IsActive(DebugLevel.Log))
                        Debug.Log($"[ImageLoader] Future[id={future.id}] Creating UnityWebRequest for loading from Source {future.Url}");

                    var asyncOperation = future.SetWebRequest(UnityWebRequestTexture.GetTexture(future.Url))
                        .SendWebRequest();

                    await UniTask.WaitUntil(() => asyncOperation.isDone || future.IsCancelled);
                }
                catch (OperationCanceledException)
                {
                    future.Cancel();
                }
                catch (TimeoutException e)
                {
                    RemoveLoading(future); // LOADING REMOVED
                    future.FailToLoad(e);
                    return;
                }
                catch (Exception e)
                {
                    if (future.LogLevel.IsActive(DebugLevel.Exception) && !ignoreImageNotFoundError)
                        Debug.LogException(e);
                }
                finally
                {
                    finished = true;
                }
            });

            try
            {
                await UniTask.WaitUntil(() => finished);
                if (future.IsCancelled || future.Status == FutureStatus.FailedToLoad)
                {
                    RemoveLoading(future); // LOADING REMOVED
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                RemoveLoading(future); // LOADING REMOVED
                future.Cancel();
                return;
            }

            if (future.WebRequest == null)
            {
                RemoveLoading(future); // LOADING REMOVED
                future.FailToLoad(new Exception($"[ImageLoader] Future[id={future.id}] UnityWebRequest is null: url={future.Url}"));
                return;
            }

#if UNITY_2020_1_OR_NEWER
            var isError = future.WebRequest.result != UnityWebRequest.Result.Success;
#else
            var isError = future.WebRequest.isNetworkError || future.WebRequest.isHttpError;
#endif
            if (isError)
            {
#if UNITY_2020_1_OR_NEWER
                var errorMessage = $"[ImageLoader] Future[id={future.id}] {future.WebRequest.result} {future.WebRequest.error}: url={future.Url}";
#else
                var errorMessage = $"[ImageLoader] Future[id={future.id}] {future.WebRequest.error}: url={future.Url}";
#endif
                RemoveLoading(future); // LOADING REMOVED
                future.FailToLoad(new Exception(errorMessage));
            }
            else
            {
                if (future.UseDiskCache)
                    await SaveDiskAsync(future.Url, future.WebRequest.downloadHandler.data);

                if (future.IsCancelled || future.Status == FutureStatus.FailedToLoad)
                {
                    RemoveLoading(future); // LOADING REMOVED
                    return;
                }
                var sprite = ToSprite(((DownloadHandlerTexture)future.WebRequest.downloadHandler).texture);
                if (future.UseMemoryCache)
                    SaveToMemoryCache(future.Url, sprite, replace: true);
                RemoveLoading(future); // LOADING REMOVED
                future.Loaded(sprite, FutureLoadedFrom.Source);
            }
        }
    }
}