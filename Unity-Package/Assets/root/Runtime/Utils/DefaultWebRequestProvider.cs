using UnityEngine.Networking;

namespace Extensions.Unity.ImageLoader
{
    /// <summary>
    /// Default implementation of IWebRequestProvider using standard UnityWebRequest
    /// Used in production builds
    /// </summary>
    public class DefaultWebRequestProvider : IWebRequestProvider
    {
        public UnityWebRequest CreateTextureRequest(string url)
        {
            return UnityWebRequestTexture.GetTexture(url);
        }

        public UnityWebRequest CreateDataRequest(string url)
        {
            return UnityWebRequest.Get(url);
        }
    }
}