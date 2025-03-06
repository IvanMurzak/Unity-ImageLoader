using System;

namespace Extensions.Unity.ImageLoader
{
    public interface IBinder<Target, Value> : IDisposable
    {
        void Set(Target target, Value value, DebugLevel logLevel = DebugLevel.Error);
    }
}
