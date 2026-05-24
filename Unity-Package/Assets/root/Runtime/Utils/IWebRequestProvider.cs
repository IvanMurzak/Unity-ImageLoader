using UnityEngine.Networking;

namespace Extensions.Unity.ImageLoader
{
    /// <summary>
    /// Interface for providing UnityWebRequest instances for image loading
    /// Allows for dependency injection and mocking during tests
    /// </summary>
    public interface IWebRequestProvider
    {
        /// <summary>
        /// Create a UnityWebRequest for loading a texture from the given URL
        /// </summary>
        /// <param name="url">URL to load texture from</param>
        /// <returns>Configured UnityWebRequest for texture loading</returns>
        UnityWebRequest CreateTextureRequest(string url);

        /// <summary>
        /// Create a UnityWebRequest for loading generic data from the given URL
        /// </summary>
        /// <param name="url">URL to load data from</param>
        /// <returns>Configured UnityWebRequest for data loading</returns>
        UnityWebRequest CreateDataRequest(string url);
    }
}