using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T>
    {
        private bool isLoadingStarted = false;
        public UniTask StartLoading(bool ignoreImageNotFoundError = false)
        {
            if (isLoadingStarted)
                return default;

            isLoadingStarted = true;
            return InternalLoading(ignoreImageNotFoundError);
        }
        internal async UniTask InternalLoading(bool ignoreImageNotFoundError = false)
        {
            if (IsCancelled || Status == FutureStatus.Disposed)
                return;

            if (string.IsNullOrEmpty(Url))
            {
                FailToLoad(new Exception($"[ImageLoader] Future[id={Id}] Empty url. Image could not be loaded!"));
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

            if (!RegisterLoading(out var anotherLoadingFuture)) // LOADING ADDED
            {
                if (LogLevel.IsActive(DebugLevel.Log))
                    Debug.Log($"[ImageLoader] Future[id={Id}] Waiting while another task is loading\n{Url}");

                anotherLoadingFuture.PassEvents(this, passCancelled: false).Forget();
                await UniTask.WaitWhile(() => IsLoading(Url) && !IsCancelled);

                if (LogLevel.IsActive(DebugLevel.Log))
                {
                    Debug.Log(IsCancelled
                        ? $"[ImageLoader] Future[id={Id}] Cancelled\n{Url}"
                        : Status == FutureStatus.FailedToLoad
                            ? $"[ImageLoader] Future[id={Id}] Another task. Failed to load\n{Url}"
                            : $"[ImageLoader] Future[id={Id}] Another task. Complete waiting for another task to load\n{Url}");
                }

                if (IsCancelled || Status == FutureStatus.FailedToLoad)
                    return;

                InternalLoading(ignoreImageNotFoundError).Forget();
                return;
            }

            if (UseDiskCache && DiskCacheContains())
            {
                Loading(FutureLoadingFrom.DiskCache);
                try
                {
                    var bytes = await LoadDiskAsync();
                    if (bytes != null && bytes.Length > 0)
                    {
                        await UniTask.SwitchToMainThread();
                        if (IsCancelled || Status == FutureStatus.FailedToLoad)
                        {
                            RemoveLoading(); // LOADING REMOVED
                            return;
                        }
                        var obj = ParseBytes(bytes);
                        if (obj != null)
                        {
                            if (UseMemoryCache)
                                SaveToMemoryCache(obj, replace: true);

                            RemoveLoading(); // LOADING REMOVED
                            if (IsCancelled || Status == FutureStatus.FailedToLoad)
                                return;
                            Loaded(obj, FutureLoadedFrom.DiskCache);
                            return;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    RemoveLoading(); // LOADING REMOVED
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

                    if (LogLevel.IsActive(DebugLevel.Trace))
                        Debug.Log($"[ImageLoader] Future[id={Id}] Creating UnityWebRequest for loading from Source\n{Url}");

                    var asyncOperation = SetWebRequest(CreateWebRequest(Url))
                        .SendWebRequest();

                    await UniTask.WaitUntil(() => asyncOperation.isDone || IsCancelled);

                    if (LogLevel.IsActive(DebugLevel.Trace))
                        Debug.Log($"[ImageLoader] Future[id={Id}] Completed UnityWebRequest for loading from Source\n{Url}");
                }
                catch (OperationCanceledException)
                {
                    if (LogLevel.IsActive(DebugLevel.Trace))
                        Debug.Log($"[ImageLoader] Future[id={Id}] Canceled UnityWebRequest for loading from Source\n{Url}");
                    Cancel();
                }
                catch (TimeoutException e)
                {
                    if (LogLevel.IsActive(DebugLevel.Trace))
                        Debug.Log($"[ImageLoader] Future[id={Id}] Timeout of UnityWebRequest for loading from Source\n{Url}");
                    RemoveLoading(); // LOADING REMOVED
                    FailToLoad(e);
                    return;
                }
                catch (Exception e)
                {
                    if (LogLevel.IsActive(DebugLevel.Trace))
                        Debug.Log($"[ImageLoader] Future[id={Id}] Exception in UnityWebRequest for loading from Source\n{Url}");

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
                    RemoveLoading(); // LOADING REMOVED
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                RemoveLoading(); // LOADING REMOVED
                Cancel();
                return;
            }

            if (WebRequest == null)
            {
                RemoveLoading(); // LOADING REMOVED
                FailToLoad(new Exception($"[ImageLoader] Future[id={Id}] UnityWebRequest is null. URL={Url}"));
                return;
            }

#if UNITY_2020_1_OR_NEWER
            var isError = WebRequest.result != UnityEngine.Networking.UnityWebRequest.Result.Success;
#else
            var isError = WebRequest.isNetworkError || WebRequest.isHttpError;
#endif
            if (isError)
            {
#if UNITY_2020_1_OR_NEWER
                var errorMessage = $"[ImageLoader] Future[id={id}] {WebRequest.result} {WebRequest.error}. URL={Url}";
#else
                var errorMessage = $"[ImageLoader] Future[id={Id}] {WebRequest.error}. URL={Url}";
#endif
                RemoveLoading(); // LOADING REMOVED
                FailToLoad(new Exception(errorMessage));
            }
            else
            {
                if (LogLevel.IsActive(DebugLevel.Log))
                    Debug.Log($"[ImageLoader] Future[id={Id}] Loaded from Source. Processing...\n{Url}");

                if (UseDiskCache)
                    await SaveDiskAsync(WebRequest.downloadHandler.data);

                if (IsCancelled || Status == FutureStatus.FailedToLoad)
                {
                    RemoveLoading(); // LOADING REMOVED
                    return;
                }
                if (LogLevel.IsActive(DebugLevel.Trace))
                    Debug.Log($"[ImageLoader] Future[id={Id}] Parsing UnityWebRequest response\n{Url}");
                var obj = ParseWebRequest(WebRequest);

                if (UseMemoryCache)
                    SaveToMemoryCache(obj, replace: true);
                RemoveLoading(); // LOADING REMOVED

                Loaded(obj, FutureLoadedFrom.Source);
            }
        }

        protected virtual bool RegisterLoading(out Future<T> anotherLoadingFuture)
            => RegisterLoading(this, out anotherLoadingFuture);
        protected virtual void RemoveLoading() => RemoveLoading(Url);
    }
}
