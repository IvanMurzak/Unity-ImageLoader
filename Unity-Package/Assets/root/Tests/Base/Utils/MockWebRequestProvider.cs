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
        // URLs routed to the deterministically held route. The request reaches the
        // server and stays in-flight until the test releases the matching id, which is
        // what makes "cancel-while-loading" scenarios deterministic.
        readonly Dictionary<string, string> heldUrlToImageId = new Dictionary<string, string>();

        public void Reset()
        {
            successUrlToImageId.Clear();
            heldUrlToImageId.Clear();
        }

        public void RegisterSuccess(string url, string imageId) => successUrlToImageId[url] = imageId;

        /// <summary>
        /// Routes <paramref name="url"/> to the server's held route (<c>/hold/{imageId}</c>).
        /// The server must have the id armed via <see cref="TestHttpServer.HoldImage"/> and
        /// the test must release it via <see cref="TestHttpServer.ReleaseHeld"/>. While held,
        /// a load of this URL stays in the LoadingFromSource state. Call <see cref="UnregisterHeld"/>
        /// (or <see cref="Reset"/>) to restore the normal fast/slow routing.
        /// </summary>
        public void RegisterHeld(string url, string imageId) => heldUrlToImageId[url] = imageId;

        public void UnregisterHeld(string url) => heldUrlToImageId.Remove(url);

        public UnityWebRequest CreateTextureRequest(string url)
            => UnityWebRequestTexture.GetTexture(ResolveUrl(url));

        public UnityWebRequest CreateDataRequest(string url)
            => UnityWebRequest.Get(ResolveUrl(url));

        string ResolveUrl(string url)
        {
            var baseUrl = TestHttpServer.Instance.BaseUrl;
            if (heldUrlToImageId.TryGetValue(url, out var heldId))
                return $"{baseUrl}/hold/{heldId}";
            return successUrlToImageId.TryGetValue(url, out var id)
                ? $"{baseUrl}/img/{id}"
                : $"{baseUrl}/slow";
        }
    }
}
