using UnityEngine;

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
#if UNITY_EDITOR && UNITY_2019_3_OR_NEWER
        [UnityEditor.InitializeOnEnterPlayMode]
        private static void ClearMemoryCacheOnEnterPlayMode() => FutureTexture.ClearMemoryCacheOnEnterPlayMode();
#endif

        /// <summary>
        /// Check the Memory cache contains sprite for the given url
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns true if Sprite exists in Memory cache</returns>
        public static bool MemoryCacheContains(string url) => FutureTexture.MemoryCacheContains(url);

        /// <summary>
        /// Save sprite to Memory cache directly. Should be used for overloading cache system
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="sprite">sprite which should be saved</param>
        /// <param name="replace">replace existed cached sprite if any</param>
        public static void SaveToMemoryCache(string url, Sprite sprite, bool replace = false) => FutureSprite.SaveToMemoryCache(url, sprite, replace);

        /// <summary>
        /// Save sprite to Memory cache directly. Should be used for overloading cache system
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="texture">sprite which should be saved</param>
        /// <param name="replace">replace existed cached sprite if any</param>
        public static void SaveToMemoryCache(string url, Texture2D texture, bool replace = false) => FutureTexture.SaveToMemoryCache(url, texture, replace);

        /// <summary>
        /// Loads directly from Memory cache if exists and allowed
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns null if not allowed to use Memory cache or if there is no cached Sprite</returns>
        public static Reference<Sprite> LoadFromMemoryCacheRef(string url) => FutureSprite.LoadFromMemoryCacheRef(url);

        /// <summary>
        /// Loads directly from Memory cache if exists and allowed
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns null if not allowed to use Memory cache or if there is no cached Sprite</returns>
        public static Sprite LoadFromMemoryCache(string url) => FutureSprite.LoadFromMemoryCache(url);

        /// <summary>
        /// Clear Memory cache for the given url
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        public static void ClearMemoryCache(string url)
        {
            FutureTexture.ClearMemoryCache(url, FutureTexture.ReleaseMemoryTexture);
            FutureSprite.ClearMemoryCache(url, FutureSprite.ReleaseMemorySprite);
        }

        /// <summary>
        /// Clear Memory cache for all urls
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        public static void ClearMemoryCacheAll()
        {
            FutureTexture.ClearMemoryCacheAll(FutureTexture.ReleaseMemoryTexture);
            FutureSprite.ClearMemoryCacheAll(FutureSprite.ReleaseMemorySprite);
        }
    }
}
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.