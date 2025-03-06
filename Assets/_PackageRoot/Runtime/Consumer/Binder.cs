using System;

namespace Extensions.Unity.ImageLoader
{
    public class Binder<Target, Value> : IBinder<Target, Value>, IDisposable
    {
        protected Action<Target, Value> setter;

        public Binder(Action<Target, Value> setter) => this.setter = setter;

        public void Set(Target target, Value value, DebugLevel logLevel = DebugLevel.Error) => Safe.Run(setter, target, value, logLevel);
        public void Dispose() => setter = null;
    }
}
