using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace Extensions.Unity.ImageLoader
{
    public abstract class ImageDecorator<T> : Decorator<Image, T>
    {

        public ImageDecorator(Image image, IFuture<T> future) : base(image, future)
        {
        }
    }
}