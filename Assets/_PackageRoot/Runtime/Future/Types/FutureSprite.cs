
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Extensions.Unity.ImageLoader
{
    public class FutureSprite : Future<Sprite>
    {
        // Overriding the default disk cache folder to save as Texture2D
        protected static new string DiskCacheFolderPath => $"{ImageLoader.settings.diskSaveLocation}/_{typeof(Texture2D).Name}";

        protected Vector2 pivot;
        protected bool mipChain;
        protected float pixelDensity;
        protected TextureFormat textureFormat;

        public FutureSprite(string url, float pixelDensity = 100, CancellationToken cancellationToken = default)
            : this(url, pivot: new Vector2(.5f, .5f), pixelDensity: pixelDensity, textureFormat: TextureFormat.ARGB32, mipChain: true, cancellationToken: cancellationToken)
        { }

        public FutureSprite(string url, Vector2 pivot, float pixelDensity = 100, TextureFormat textureFormat = TextureFormat.ARGB32, bool mipChain = true, CancellationToken cancellationToken = default)
            : base(url, cancellationToken)
        {
            this.pivot = pivot;
            this.mipChain = mipChain;
            this.pixelDensity = pixelDensity;
            this.textureFormat = textureFormat;
        }

        // --- Disc Cache ---
        protected override bool DiskCacheContains() => FutureTexture.DiskCacheContains(Url);
        protected override Task<byte[]> LoadDiskAsync()
        {
            if (LogLevel.IsActive(DebugLevel.Log))
                Debug.Log($"[ImageLoader] Future[id={Id}] Load from Disk cache ({typeof(Texture2D).Name} -> {typeof(Sprite).Name})\n{Url}");
            return FutureTexture.LoadDiskAsync(Url, DebugLevel.Error);
        }
        protected override Task SaveDiskAsync(byte[] data)
        {
            if (LogLevel.IsActive(DebugLevel.Log))
                Debug.Log($"[ImageLoader] Future[id={Id}] Save to Disk cache ({typeof(Texture2D).Name} <- {typeof(Sprite).Name})\n{Url}");
            return FutureTexture.SaveDiskAsync(Url, data, DebugLevel.Error);
        }

        // --- Memory Cache ---
        protected override Sprite LoadFromMemoryCache()
        {
            var sprite = base.LoadFromMemoryCache();
            if (sprite == null)
            {
                var texture = FutureTexture.LoadFromMemoryCache(Url);
                if (texture != null)
                {
                    sprite = texture.ToSprite(pivot, pixelDensity);
                    if (sprite != null)
                        SaveToMemoryCache(sprite);
                }
            }

            return sprite;
        }
        protected override void SaveToMemoryCache(Sprite obj, bool replace = false)
        {
            FutureTexture.SaveToMemoryCache(Url, obj.texture, replace, suppressMessage: true);
            base.SaveToMemoryCache(obj, replace);
        }
        protected override void ReleaseMemory(Sprite obj, DebugLevel logLevel = DebugLevel.Log) => ReleaseMemorySprite(obj, logLevel);

        public static void ReleaseMemorySprite(Sprite sprite, DebugLevel logLevel = DebugLevel.Log)
        {
            if (PlayerLoopHelper.IsMainThread)
            {
                if (sprite.IsNull())
                    return;

                if (sprite.texture.IsNotNull())
                {
                    if (logLevel.IsActive(DebugLevel.Trace))
                        Debug.Log($"[ImageLoader] Release memory Sprite->Texture2D");
                    UnityEngine.Object.DestroyImmediate(sprite.texture);
                }
                UnityEngine.Object.DestroyImmediate(sprite);
                return;
            }

            // 'sprite.texture' could be called only from main thread
            // checking sprite.IsNull is enough until we are in main thread
            if (sprite.IsNull())
                return;

            UniTask.Post(() =>
            {
                if (sprite.IsNull()) // double check after async delay
                    return;

                if (sprite.texture.IsNotNull()) // double check after async delay
                {
                    if (logLevel.IsActive(DebugLevel.Trace))
                        Debug.Log($"[ImageLoader] Release memory Sprite->Texture2D");
                    UnityEngine.Object.DestroyImmediate(sprite.texture);
                }
                UnityEngine.Object.DestroyImmediate(sprite);
            });
        }

        // --- Web Request ---
        protected override UnityWebRequest CreateWebRequest(string url)
            => UnityWebRequestTexture.GetTexture(url);
        protected override Sprite ParseWebRequest(UnityWebRequest webRequest)
            => ImageLoader.ToSprite(DownloadHandlerTexture.GetContent(webRequest), pivot);
        protected override Sprite ParseBytes(byte[] bytes)
            => ImageLoader.ToSprite(FutureTexture.ParseBytesToTexture(bytes, textureFormat, mipChain), pivot);
    }
}