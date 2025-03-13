using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Extensions.Unity.ImageLoader
{
    public class Decorator<C, T> : IDisposable
    {
        public C Consumer { get; private set; }
        public IFuture<T> Future { get; private set; }

        public Decorator(C consumer, IFuture<T> future)
        {
            Consumer = consumer;
            Future = future;
        }

        public void Dispose()
        {
            Future = null;
            Consumer = default;
        }
    }
}