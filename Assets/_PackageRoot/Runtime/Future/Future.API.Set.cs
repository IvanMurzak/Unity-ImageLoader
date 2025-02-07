using System;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T> : IDisposable
    {
        /// <summary>
        /// Set disk cache usage for this Future instance
        /// </summary>
        /// <param name="value">new value</param>
        /// <returns>Returns the Future instance</returns>
        public Future<T> SetUseDiskCache(bool value = true)
        {
            UseDiskCache = value;
            return this;
        }

        /// <summary>
        /// Set memory cache usage for this Future instance
        /// </summary>
        /// <param name="value">new value</param>
        /// <returns>Returns the Future instance</returns>
        public Future<T> SetUseMemoryCache(bool value = true)
        {
            UseMemoryCache = value;
            return this;
        }
    }
}
