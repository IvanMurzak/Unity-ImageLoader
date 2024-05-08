using System;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T> : IDisposable
    {
        internal Future<T> PassEvents(Future<T> to)
        {
            OnLoadedFromMemoryCache += (v) => to.Loaded(v, FutureLoadedFrom.MemoryCache);
            OnLoadingFromDiskCache  += ( ) => to.Loading(FutureLoadingFrom.DiskCache);
            OnLoadedFromDiskCache   += (v) => to.Loaded(v, FutureLoadedFrom.DiskCache);
            OnLoadingFromSource     += ( ) => to.Loading(FutureLoadingFrom.Source);
            OnLoadedFromSource      += (v) => to.Loaded(v, FutureLoadedFrom.Source);
            OnFailedToLoad          += to.FailToLoad;
            OnCancelled             += to.Cancel;

            return this;
        }
    }
}
