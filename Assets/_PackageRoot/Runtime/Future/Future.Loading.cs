using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T>
    {
        // Vector2 pivot, TextureFormat textureFormat = TextureFormat.ARGB32,
        /*
            parse = (bytes) =>
            {
                var texture = new Texture2D(2, 2, textureFormat, true);
                if (texture.LoadImage(bytes))
                {
                    return ToSprite(texture, pivot);
                }
                return null;
            }

            parseWebRequest = (webRequest) =>
            {
                ToSprite(((DownloadHandlerTexture)webRequest.downloadHandler).texture)

                var texture = DownloadHandlerTexture.GetContent(webRequest);
                return ToSprite(texture, pivot);
            }
        */
        private bool isLoadingStarted = false;
        public UniTaskVoid StartLoading(Func<string, UnityWebRequest> createWebRequest, Func<byte[], T> parseBytes, Func<UnityWebRequest, T> parseWebRequest, bool ignoreImageNotFoundError = false)
        {
            if (isLoadingStarted)
                return default;

            isLoadingStarted = true;
            return InternalLoading(createWebRequest, parseBytes, parseWebRequest, ignoreImageNotFoundError);
        }
        internal async UniTaskVoid InternalLoading(Func<string, UnityWebRequest> createWebRequest, Func<byte[], T> parseBytes, Func<UnityWebRequest, T> parseWebRequest, bool ignoreImageNotFoundError = false)
        {
            if (IsCancelled || Status == FutureStatus.Disposed)
                return;

            if (string.IsNullOrEmpty(Url))
            {
                FailToLoad(new Exception($"[ImageLoader] Future[id={id}] Empty url. Image could not be loaded!"));
                return;
            }

            if (UseMemoryCache && MemoryCacheContains(Url))
            {
                var obj = LoadFromMemoryCache(Url);
                if (obj != null)
                {
                    Loaded(obj, FutureLoadedFrom.MemoryCache);
                    return;
                }
            }

            var anotherLoadingFuture = GetLoadingFuture(Url);
            if (anotherLoadingFuture != null)
            {
                if (LogLevel.IsActive(DebugLevel.Log))
                    Debug.Log($"[ImageLoader] Future[id={id}] Waiting while another task is loading {Url}");

                anotherLoadingFuture.PassEvents(this, passCancelled: false).Forget();
                await UniTask.WaitWhile(() => IsLoading(Url) && !IsCancelled);

                if (LogLevel.IsActive(DebugLevel.Log))
                {
                    Debug.Log(IsCancelled
                        ? $"[ImageLoader] Future[id={id}] Cancelled {Url}"
                        : Status == FutureStatus.FailedToLoad
                            ? $"[ImageLoader] Future[id={id}] Another task. Failed to load {Url}"
                            : $"[ImageLoader] Future[id={id}] Another task. Complete waiting for another task to load {Url}");
                }

                if (IsCancelled || Status == FutureStatus.FailedToLoad)
                    return;

                InternalLoading(createWebRequest, parseBytes, parseWebRequest, ignoreImageNotFoundError).Forget();
                return;
            }

            AddLoading(this); // LOADING ADDED

            if (UseDiskCache && DiskCacheContains(Url))
            {
                Loading(FutureLoadingFrom.DiskCache);
                try
                {
                    var bytes = await LoadDiskAsync(Url);
                    if (bytes != null && bytes.Length > 0)
                    {
                        await UniTask.SwitchToMainThread();
                        if (IsCancelled || Status == FutureStatus.FailedToLoad)
                        {
                            RemoveLoading(this); // LOADING REMOVED
                            return;
                        }
                        var obj = parseBytes(bytes);
                        if (obj != null)
                        {
                            if (UseMemoryCache)
                                SaveToMemoryCache(Url, obj, replace: true);

                            RemoveLoading(this); // LOADING REMOVED
                            if (IsCancelled || Status == FutureStatus.FailedToLoad)
                                return;
                            Loaded(obj, FutureLoadedFrom.DiskCache);
                            return;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    RemoveLoading(this); // LOADING REMOVED
                    Cancel();
                    return;
                }
                catch (Exception e)
                {
                    if (LogLevel.IsActive(DebugLevel.Exception))
                        Debug.LogException(e);
                }
            }

            Loading(FutureLoadingFrom.Source);

            var finished = false;
            UniTask.Post(async () =>
            {
                try
                {
                    if (IsCancelled || Status == FutureStatus.FailedToLoad)
                        return;

                    if (LogLevel.IsActive(DebugLevel.Log))
                        Debug.Log($"[ImageLoader] Future[id={id}] Creating UnityWebRequest for loading from Source {Url}");


                    var asyncOperation = SetWebRequest(createWebRequest(Url))
                        .SendWebRequest();

                    await UniTask.WaitUntil(() => asyncOperation.isDone || IsCancelled);
                }
                catch (OperationCanceledException)
                {
                    Cancel();
                }
                catch (TimeoutException e)
                {
                    RemoveLoading(this); // LOADING REMOVED
                    FailToLoad(e);
                    return;
                }
                catch (Exception e)
                {
                    if (LogLevel.IsActive(DebugLevel.Exception) && !ignoreImageNotFoundError)
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
                if (IsCancelled || Status == FutureStatus.FailedToLoad)
                {
                    RemoveLoading(this); // LOADING REMOVED
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                RemoveLoading(this); // LOADING REMOVED
                Cancel();
                return;
            }

            if (WebRequest == null)
            {
                RemoveLoading(this); // LOADING REMOVED
                FailToLoad(new Exception($"[ImageLoader] Future[id={id}] UnityWebRequest is null: url={Url}"));
                return;
            }

#if UNITY_2020_1_OR_NEWER
            var isError = WebRequest.result != UnityWebRequest.Result.Success;
#else
            var isError = WebRequest.isNetworkError || WebRequest.isHttpError;
#endif
            if (isError)
            {
#if UNITY_2020_1_OR_NEWER
                var errorMessage = $"[ImageLoader] Future[id={id}] {WebRequest.result} {WebRequest.error}: url={Url}";
#else
                var errorMessage = $"[ImageLoader] Future[id={id}] {WebRequest.error}: url={Url}";
#endif
                RemoveLoading(this); // LOADING REMOVED
                FailToLoad(new Exception(errorMessage));
            }
            else
            {
                if (UseDiskCache)
                    await SaveDiskAsync(Url, WebRequest.downloadHandler.data);

                if (IsCancelled || Status == FutureStatus.FailedToLoad)
                {
                    RemoveLoading(this); // LOADING REMOVED
                    return;
                }
                var obj = parseWebRequest(WebRequest);
                if (UseMemoryCache)
                    SaveToMemoryCache(Url, obj, replace: true);
                RemoveLoading(this); // LOADING REMOVED
                Loaded(obj, FutureLoadedFrom.Source);
            }
        }
    }
}
