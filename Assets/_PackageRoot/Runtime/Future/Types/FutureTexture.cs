
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Extensions.Unity.ImageLoader
{
    public class FutureTexture : Future<Texture2D>
    {
        protected bool mipChain;
        protected TextureFormat textureFormat;
        public FutureTexture(string url, TextureFormat textureFormat = TextureFormat.ARGB32, bool mipChain = true, CancellationToken cancellationToken = default) : base(url, cancellationToken)
        {
            this.mipChain = mipChain;
            this.textureFormat = textureFormat;
        }

        // --- Disc Cache ---
        protected override Task<byte[]> LoadDiskAsync()
        {
            if (LogLevel.IsActive(DebugLevel.Log))
                Debug.Log($"[ImageLoader] Future[id={id}] Load from Disk cache ({typeof(Texture2D).Name})\n{Url}");
            return base.LoadDiskAsync();
        }
        protected override Task SaveDiskAsync(byte[] data)
        {
            if (LogLevel.IsActive(DebugLevel.Log))
                Debug.Log($"[ImageLoader] Future[id={id}] Save to Disk cache ({typeof(Texture2D).Name})\n{Url}");
            return base.SaveDiskAsync(data);
        }

        // --- Memory Cache ---
        protected override void ReleaseMemory(Texture2D obj) => ReleaseMemoryTexture(obj);

        public static void ReleaseMemoryTexture(Texture2D obj)
        {
            if (!ReferenceEquals(obj, null) && obj != null)
                UnityEngine.Object.DestroyImmediate(obj);
        }

        // --- Web Request ---
        protected override UnityWebRequest CreateWebRequest(string url)
            => UnityWebRequestTexture.GetTexture(url);
        protected override Texture2D ParseWebRequest(UnityWebRequest webRequest)
            => DownloadHandlerTexture.GetContent(webRequest);
        protected override Texture2D ParseBytes(byte[] bytes)
            => ParseBytesToTexture(bytes, textureFormat, mipChain);

        public static Texture2D ParseBytesToTexture(byte[] bytes, TextureFormat textureFormat = TextureFormat.ARGB32, bool mipChain = true)
        {
            var texture = new Texture2D(2, 2, textureFormat, mipChain);
            if (texture.LoadImage(bytes))
                return texture;
            return null;
        }
    }
}