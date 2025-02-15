namespace Extensions.Unity.ImageLoader
{
    public abstract partial class Future<T>
    {
        /// <summary>
        /// Save sprite to Memory cache directly. Should be used for overloading cache system
        /// </summary>
        /// <param name="obj">sprite which should be saved</param>
        /// <param name="replace">replace existed cached sprite if any</param>
        protected virtual void SaveToMemoryCache(T obj, bool replace = false) => SaveToMemoryCache(Url, obj, replace);
        /// <summary>
        /// Loads directly from Memory cache if exists and allowed
        /// </summary>
        /// <returns>Returns null if not allowed to use Memory cache or if there is no cached Sprite</returns>
        protected virtual T LoadFromMemoryCache() => LoadFromMemoryCache(Url);
        /// <summary>
        /// Clear Memory cache for the given url
        /// </summary>
        protected virtual void ClearMemoryCache() => ClearMemoryCache(Url, ReleaseMemory);
        /// <summary>
        /// Clear Memory cache for all urls
        /// </summary>
        protected virtual void ClearMemoryCacheAll() => ClearMemoryCacheAll(ReleaseMemory);

        protected abstract void ReleaseMemory(T obj);
    }
}
