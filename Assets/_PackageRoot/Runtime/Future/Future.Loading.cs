using System;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T>
    {
        private bool isLoadingStarted = false;
        public UniTaskVoid StartLoading(bool ignoreImageNotFoundError = false)
        {
            if (isLoadingStarted)
                return default;

            isLoadingStarted = true;
            return InternalLoading(ignoreImageNotFoundError);
        }
        internal async UniTaskVoid InternalLoading(bool ignoreImageNotFoundError = false)
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

            if (!RegisterLoading(out var anotherLoadingFuture)) // LOADING ADDED
            {
                if (LogLevel.IsActive(DebugLevel.Log))
                    Debug.Log($"[ImageLoader] Future[id={id}] Waiting while another task is loading\n{Url}");

                anotherLoadingFuture.PassEvents(this, passCancelled: false).Forget();
                await UniTask.WaitWhile(() => IsLoading(Url) && !IsCancelled);

                if (LogLevel.IsActive(DebugLevel.Log))
                {
                    Debug.Log(IsCancelled
                        ? $"[ImageLoader] Future[id={id}] Cancelled\n{Url}"
                        : Status == FutureStatus.FailedToLoad
                            ? $"[ImageLoader] Future[id={id}] Another task. Failed to load\n{Url}"
                            : $"[ImageLoader] Future[id={id}] Another task. Complete waiting for another task to load\n{Url}");
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

                    if (LogLevel.IsActive(DebugLevel.Log))
                        Debug.Log($"[ImageLoader] Future[id={id}] Creating UnityWebRequest for loading from Source\n{Url}");


                    var asyncOperation = SetWebRequest(CreateWebRequest(Url))
                        .SendWebRequest();

                    await UniTask.WaitUntil(() => asyncOperation.isDone || IsCancelled);
                }
                catch (OperationCanceledException)
                {
                    Cancel();
                }
                catch (TimeoutException e)
                {
                    RemoveLoading(); // LOADING REMOVED
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
                RemoveLoading(); // LOADING REMOVED
                FailToLoad(new Exception(errorMessage));
            }
            else
            {
                if (UseDiskCache)
                    await SaveDiskAsync(WebRequest.downloadHandler.data);

                if (IsCancelled || Status == FutureStatus.FailedToLoad)
                {
                    RemoveLoading(); // LOADING REMOVED
                    return;
                }
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
