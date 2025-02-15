
using System.Threading;
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

        protected override Texture2D ParseWebRequest(UnityWebRequest webRequest)
            => DownloadHandlerTexture.GetContent(webRequest);

        protected override void ReleaseMemory(Texture2D obj) => ReleaseMemoryTexture(obj);

        public static void ReleaseMemoryTexture(Texture2D obj)
        {
            if (!ReferenceEquals(obj, null) && obj != null)
                UnityEngine.Object.DestroyImmediate(obj);
        }

        protected override Texture2D ParseBytes(byte[] bytes)
            => ParseBytesToTexture(bytes, textureFormat, mipChain);

        public static Texture2D ParseBytesToTexture(byte[] bytes, TextureFormat textureFormat = TextureFormat.ARGB32, bool mipChain = true)
        {
            var texture = new Texture2D(2, 2, textureFormat, mipChain);
            if (texture.LoadImage(bytes))
                return texture;
            return null;
        }

        protected override UnityWebRequest CreateWebRequest(string url)
            => UnityWebRequestTexture.GetTexture(url);
    }
}