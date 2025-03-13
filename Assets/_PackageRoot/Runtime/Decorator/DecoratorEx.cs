using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace Extensions.Unity.ImageLoader
{
    public static class Decorator
    {
        public static Decorator<Image, Sprite> LoadedFadeIn(this Decorator<Image, Sprite> decorator, Image image)
        {
            decorator.Future.Loaded(sprite => image.FadeIn(sprite));

            return decorator;
        }
    }
}