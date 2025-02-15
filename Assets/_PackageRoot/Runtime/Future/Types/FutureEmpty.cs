
using System.Threading;
using UnityEngine.Networking;

namespace Extensions.Unity.ImageLoader
{
    public class FutureEmpty<T> : Future<T>
    {
        public FutureEmpty(string url, CancellationToken cancellationToken = default) : base(url, cancellationToken)
        {

        }

        protected override UnityWebRequest CreateWebRequest(string url)
            => throw new System.NotImplementedException();

        protected override T ParseBytes(byte[] bytes) => default;

        protected override T ParseWebRequest(UnityWebRequest webRequest) => default;

        protected override void ReleaseMemory(T obj) { }
    }
}