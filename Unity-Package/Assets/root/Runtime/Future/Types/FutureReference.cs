
using System.Threading;
using UnityEngine.Networking;

namespace Extensions.Unity.ImageLoader
{
    public class FutureReference<T> : Future<Reference<T>>
    {
        public FutureReference(string url, CancellationToken cancellationToken = default, DebugLevel? logLevel = null) : base(url, cancellationToken, logLevel)
        {

        }

        protected override UnityWebRequest CreateWebRequest(string url) => throw new System.NotImplementedException();
        protected override Reference<T> ParseBytes(byte[] bytes) => throw new System.NotImplementedException();
        protected override Reference<T> ParseWebRequest(UnityWebRequest webRequest) => throw new System.NotImplementedException();

        protected override void ReleaseMemory(Reference<T> obj, DebugLevel logLevel = DebugLevel.Log)
        {
            // TODO: Implement this method
        }
    }
}