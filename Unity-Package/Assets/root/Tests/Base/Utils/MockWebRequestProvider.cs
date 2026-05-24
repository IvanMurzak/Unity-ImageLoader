using System.Collections.Generic;
using UnityEngine.Networking;

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    /// <summary>
    /// Test <see cref="IWebRequestProvider"/> that routes every request to the
    /// in-process <see cref="TestHttpServer"/> on 127.0.0.1 instead of the public
    /// internet. Registered URLs resolve to a fast, deterministic local image; any
    /// other URL (e.g. the random doesntexist.com URLs used by failing tests)
    /// resolves to the server's slow route so the client-side Future timeout fires
    /// predictably.
    /// </summary>
    public class MockWebRequestProvider : IWebRequestProvider
    {
        static MockWebRequestProvider instance;
        public static MockWebRequestProvider Instance
        {
            get
            {
                if (instance == null) instance = new MockWebRequestProvider();
                return instance;
            }
        }

        readonly Dictionary<string, string> successUrlToImageId = new Dictionary<string, string>();

        public void Reset() => successUrlToImageId.Clear();

        public void RegisterSuccess(string url, string imageId) => successUrlToImageId[url] = imageId;

        public UnityWebRequest CreateTextureRequest(string url)
            => UnityWebRequestTexture.GetTexture(ResolveUrl(url));

        public UnityWebRequest CreateDataRequest(string url)
            => UnityWebRequest.Get(ResolveUrl(url));

        string ResolveUrl(string url)
        {
            var baseUrl = TestHttpServer.Instance.BaseUrl;
            return successUrlToImageId.TryGetValue(url, out var id)
                ? $"{baseUrl}/img/{id}"
                : $"{baseUrl}/slow";
        }
    }
}
