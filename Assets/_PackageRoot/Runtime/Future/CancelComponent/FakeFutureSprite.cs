using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    public class FakeFutureSprite : FutureSprite
    {
        public FakeFutureSprite(string url, float pixelDensity = 100, CancellationToken cancellationToken = default)
            : this(url, pivot: new Vector2(.5f, .5f), pixelDensity: pixelDensity, textureFormat: TextureFormat.ARGB32, mipChain: true, cancellationToken: cancellationToken)
        { }

        public FakeFutureSprite(string url, Vector2 pivot, float pixelDensity = 100, TextureFormat textureFormat = TextureFormat.ARGB32, bool mipChain = true, CancellationToken cancellationToken = default)
            : base(url, pivot: pivot, pixelDensity: pixelDensity, textureFormat: TextureFormat.ARGB32, mipChain: true, cancellationToken: cancellationToken)
        { }

        protected override UnityWebRequest CreateWebRequest(string url) => new FakeUnityWebRequest(url);
    }
}