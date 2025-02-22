using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    public class FakeFutureTexture : FutureTexture
    {
        public FakeFutureTexture(string url, TextureFormat textureFormat = TextureFormat.ARGB32, bool mipChain = true, CancellationToken cancellationToken = default)
            : base(url, textureFormat, mipChain, cancellationToken)
        {
        }

        protected override UnityWebRequest CreateWebRequest(string url) => new FakeUnityWebRequest(url);
    }
}