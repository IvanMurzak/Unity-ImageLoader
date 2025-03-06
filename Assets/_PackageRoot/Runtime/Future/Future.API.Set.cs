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

        /// <summary>
        /// Set the setter function
        /// </summary>
        /// <param name="setter">Setter function</param>
        /// <returns>Returns the Future instance</returns>
        public IFuture<T> Set(Action<T> setter)
        {
            lock (setters)
            {
                setters.Clear();
                setters.Add(setter);
            }

            if (IsLoaded)
            {
                Safe.Run(setter, value, LogLevel);
                return this;
            }
            return this;
        }
        /// <summary>
        /// Set or add the setter function
        /// </summary>
        /// <param name="setter">Setter function</param>
        /// <returns>Returns the Future instance</returns>
        public IFuture<T> SetOrAdd(Action<T> setter)
        {
            lock (setters)
                setters.Add(setter);

            if (IsLoaded)
            {
                Safe.Run(setter, value, LogLevel);
                return this;
            }
            return this;
        }
    }
}
