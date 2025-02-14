
using System.Threading;

namespace Extensions.Unity.ImageLoader
{
    public class FutureEmpty<T> : Future<T>
    {
        public FutureEmpty(string url, CancellationToken cancellationToken) : base(url, cancellationToken)
        {

        }

        protected override void ReleaseMemory(T obj)
        {

        }
    }
}