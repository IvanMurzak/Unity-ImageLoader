
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
        protected TextureFormat textureFormat;

        public FutureSprite(string url, CancellationToken cancellationToken = default)
            : this(url, new Vector2(.5f, .5f), TextureFormat.ARGB32, mipChain: true, cancellationToken: cancellationToken)
        {

        }

        public FutureSprite(string url, Vector2 pivot, TextureFormat textureFormat = TextureFormat.ARGB32, bool mipChain = true, CancellationToken cancellationToken = default) : base(url, cancellationToken)
        {
            this.pivot = pivot;
            this.mipChain = mipChain;
            this.textureFormat = textureFormat;
        }

        // --- Disc Cache ---
        protected override bool DiskCacheContains() => FutureTexture.DiskCacheContains(Url);
        protected override Task<byte[]> LoadDiskAsync()
        {
            if (LogLevel.IsActive(DebugLevel.Log))
                Debug.Log($"[ImageLoader] Future[id={Id}] Load from Disk cache ({typeof(Texture2D).Name} -> {typeof(Sprite).Name})\n{Url}");
            return FutureTexture.LoadDiskAsync(Url, suppressMessage: true);
        }
        protected override Task SaveDiskAsync(byte[] data)
        {
            if (LogLevel.IsActive(DebugLevel.Log))
                Debug.Log($"[ImageLoader] Future[id={Id}] Save to Disk cache ({typeof(Texture2D).Name} <- {typeof(Sprite).Name})\n{Url}");
            return FutureTexture.SaveDiskAsync(Url, data, suppressMessage: true);
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
                    sprite = ImageLoader.ToSprite(texture);
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

        public static void ReleaseMemorySprite(Sprite obj, DebugLevel logLevel = DebugLevel.Log)
        {
            if (!ReferenceEquals(obj, null) && obj != null)
            {
                if (logLevel.IsActive(DebugLevel.Log))
                    Debug.Log($"[ImageLoader] Release memory Sprite->Texture2D");
                UniTask.Post(() => FutureTexture.ReleaseMemoryTexture(obj.texture, logLevel));
            }
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