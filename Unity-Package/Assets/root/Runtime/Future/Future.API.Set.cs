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
        public IFuture<T> SetUseDiskCache(bool value = true)
        {
            UseDiskCache = value;
            return this;
        }

        /// <summary>
        /// Set memory cache usage for this Future instance
        /// </summary>
        /// <param name="value">new value</param>
        /// <returns>Returns the Future instance</returns>
        public IFuture<T> SetUseMemoryCache(bool value = true)
        {
            UseMemoryCache = value;
            return this;
        }

        /// <summary>
        /// Set log level for this Future instance
        /// </summary>
        /// <param name="value">new value</param>
        /// <returns>Returns the Future instance</returns>
        public IFuture<T> SetLogLevel(DebugLevel value)
        {
            LogLevel = value;
            return this;
        }
    }
}
