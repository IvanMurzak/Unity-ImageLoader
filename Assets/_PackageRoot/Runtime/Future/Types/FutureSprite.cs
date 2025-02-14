
using System.Threading;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public class FutureSprite : Future<Sprite>
    {
        // Overriding the default disk cache folder to save as Texture2D
        protected static new string DiskCacheFolderPath => $"{ImageLoader.settings.diskSaveLocation}/_{typeof(Texture2D).Name}";

        public FutureSprite(string url, CancellationToken cancellationToken) : base(url, cancellationToken)
        {

        }

        protected override void ReleaseMemory(Sprite obj) => ReleaseMemorySprite(obj);

        public static void ReleaseMemorySprite(Sprite obj)
        {
            if (!ReferenceEquals(obj, null) && obj != null &&
                !ReferenceEquals(obj.texture, null) && obj.texture != null)
                UnityEngine.Object.DestroyImmediate(obj.texture);
        }
    }
}