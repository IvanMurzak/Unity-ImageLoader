
using System.Threading;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public class FutureTexture : Future<Texture2D>
    {
        public FutureTexture(string url, CancellationToken cancellationToken) : base(url, cancellationToken)
        {

        }

        protected override void ReleaseMemory(Texture2D obj) => ReleaseMemoryTexture(obj);

        public static void ReleaseMemoryTexture(Texture2D obj)
        {
            if (!ReferenceEquals(obj, null) && obj != null)
                UnityEngine.Object.DestroyImmediate(obj);
        }
    }
}