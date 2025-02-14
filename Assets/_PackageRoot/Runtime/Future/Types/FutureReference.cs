
using System.Threading;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public class FutureReference<T> : Future<Reference<T>>
    {
        public FutureReference(string url, CancellationToken cancellationToken) : base(url, cancellationToken)
        {

        }

        protected override void ReleaseMemory(Reference<T> obj)
        {
            // TODO: Implement this method
        }
    }
}