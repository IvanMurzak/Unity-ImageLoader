using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

namespace Extensions.Unity.ImageLoader
{
    public static class ImageLoader
    {
#region Private
        private static HashSet<string> loadingInProcess = new HashSet<string>();
        private static Dictionary<string, Sprite> loadedSpritesCache = new Dictionary<string, Sprite>();

        private static readonly TaskFactory factory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(1));
        private static string CachePath(string url) => $"{SaveLocation}/I_{url.GetHashCode()}";
        private static void Save(string url, byte[] data)
        {
            Directory.CreateDirectory(SaveLocation);
            Directory.CreateDirectory(Path.GetDirectoryName(CachePath(url)));
            File.WriteAllBytes(CachePath(url), data);
        }
        private static byte[] Load(string url)
        {
            Directory.CreateDirectory(SaveLocation);
            Directory.CreateDirectory(Path.GetDirectoryName(CachePath(url)));
            if (!File.Exists(CachePath(url))) return null;
            return File.ReadAllBytes(CachePath(url));
        }
        private static Task SaveAsync(string url, byte[] data) => factory.StartNew(() => Save(url, data));
        private static Task<byte[]> LoadAsync(string url) => factory.StartNew(() => Load(url));
#endregion

        public static string SaveLocation { get; set; } = Application.persistentDataPath + "/imageCache";

        public static void Init()
        {
            // need get SaveLocation variable in runtime from thread to setup the default static value into it
            var temp = SaveLocation + SaveLocation;
        }

        public static void ClearCacheOnDisk() => Directory.Delete(SaveLocation, true);
        public static void ClearCacheInMemory()
        {
            foreach (var cache in loadedSpritesCache.Values)
            {
                if (cache?.texture != null)
                    UnityEngine.Object.Destroy(cache.texture);
            }
            loadedSpritesCache.Clear();
        }
        public static void ClearCacheAll()
        {
            ClearCacheOnDisk();
            ClearCacheInMemory();
        }

        public static Sprite ToSprite(Texture2D texture, float pixelDensity = 100f) => Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelDensity);
        public static Sprite ToSprite(Texture2D texture, Vector2 pivot, float pixelDensity = 100f) => Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), pivot, pixelDensity);

        public static async UniTask<Sprite> LoadSprite(string url, bool ignoreImageNotFoundError = false)
        {
            if (string.IsNullOrEmpty(url))
            {
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
                Debug.Log($"ImageLoader: Waiting while another task is loading the sprite url={url}");
                await UniTask.WaitWhile(() => loadingInProcess.Contains(url));
                return await LoadSprite(url, ignoreImageNotFoundError);
            }
            loadingInProcess.Add(url);

            Debug.Log($"ImageLoader: Loading new Sprite into memory url={url}");
            try
            {
                var cachedImage = await LoadAsync(url);
                if (cachedImage != null && cachedImage.Length > 0)
                {
                    await UniTask.SwitchToMainThread();
                    var texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
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
                Debug.LogException(e);
            }
            finally
            {
                loadingInProcess.Remove(url);
            }

            UnityWebRequest request = null;
            bool finished = false;
            UniTask.Post(async () =>
            {
                if (ignoreImageNotFoundError)
                {
                    try
                    {
                        request = UnityWebRequestTexture.GetTexture(url);
                        await request.SendWebRequest();
                    }
                    catch (Exception e) { Debug.LogException(e); }
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
                    catch (Exception e) { Debug.LogException(e); }
                });
            }
            catch (Exception e) { Debug.LogException(e); }
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
                    catch (Exception e) { Debug.LogException(e); }
                }
            });
        }
    }
}