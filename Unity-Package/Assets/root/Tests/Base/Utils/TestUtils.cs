using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    public static partial class TestUtils
    {
        public static readonly string[] ImageURLs =
        {
            "https://github.com/IvanMurzak/Unity-ImageLoader/raw/main/Test%20Images/ImageA.jpg",
            "https://github.com/IvanMurzak/Unity-ImageLoader/raw/main/Test%20Images/ImageB.png",
            "https://github.com/IvanMurzak/Unity-ImageLoader/raw/main/Test%20Images/ImageC.png"
        };
        public static string IncorrectImageURL => $"https://doesntexist.com/{Guid.NewGuid()}.png";
        public static IEnumerable<string> IncorrectImageURLs(int count = 3) => Enumerable.Range(0, count).Select(_ => IncorrectImageURL);
        public static readonly byte[] CorruptedTextureBytes = new byte[] { 0 };

        public static readonly Dictionary<PlaceholderTrigger, Sprite> placeholderSprites = new Dictionary<PlaceholderTrigger, Sprite>
        {
            { PlaceholderTrigger.LoadingFromDiskCache, Texture2D.whiteTexture.ToSprite() },
            { PlaceholderTrigger.LoadingFromSource, Texture2D.blackTexture.ToSprite() },
            { PlaceholderTrigger.FailedToLoad, Texture2D.redTexture.ToSprite() },
            { PlaceholderTrigger.Canceled, Texture2D.grayTexture.ToSprite() }
        };

        public static IEnumerator ClearEverything(string message)
        {
            if (message != null)
                Debug.Log(message.PadRight(50, '-'));

            LogAssert.ignoreFailingMessages = true; // compilation error in player build

            UniTaskScheduler.UnobservedExceptionWriteLogType = LogType.Exception;

            // Enable mock provider for tests
            EnableMockProvider();

            ImageLoader.ClearSpriteRef();
            ImageLoader.ClearTextureRef();
            yield return ImageLoader.ClearCacheAll().TimeoutCoroutine(TimeSpan.FromSeconds(10));

            WaitForGCFast();

            LogAssert.ignoreFailingMessages = false; // compilation error in player build
        }
        public static void WaitForGCFast()
        {
            GC.Collect(100, GCCollectionMode.Forced, blocking: true);
            GC.WaitForPendingFinalizers();
        }
        public static IEnumerator WaitForGC(int millisecondsDelay = 100)
        {
            yield return Wait(TimeSpan.FromMilliseconds(millisecondsDelay));
            WaitForGCFast();
            yield return Wait(TimeSpan.FromMilliseconds(millisecondsDelay));
        }
        public static IEnumerator RunNoLogs(Func<IEnumerator> test)
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            yield return test();
        }
        public static void RunNoLogs(Action test)
        {
            ImageLoader.settings.debugLevel = DebugLevel.Error;
            test();
        }

        public static Texture2D CreateTestTexture(int width = 64, int height = 64, Color? color = null)
        {
            var testColor = color ?? Color.green;
            var texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            var pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = testColor;
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        static readonly Color[] mockColors =
        {
            Color.green, Color.blue, Color.red, Color.cyan, Color.magenta, Color.yellow
        };
        static byte[][] mockPngCache;

        public static void EnableMockProvider()
        {
            TestHttpServer.Instance.EnsureStarted();

            if (mockPngCache == null)
            {
                mockPngCache = new byte[ImageURLs.Length][];
                for (int i = 0; i < ImageURLs.Length; i++)
                {
                    var texture = CreateTestTexture(64, 64, mockColors[i % mockColors.Length]);
                    mockPngCache[i] = texture.EncodeToPNG();
                }
            }

            MockWebRequestProvider.Instance.Reset();
            for (int i = 0; i < ImageURLs.Length; i++)
            {
                var id = i.ToString();
                TestHttpServer.Instance.RegisterImage(id, mockPngCache[i]);
                MockWebRequestProvider.Instance.RegisterSuccess(ImageURLs[i], id);
            }
            ImageLoader.settings.webRequestProvider = MockWebRequestProvider.Instance;
        }

        public static void DisableMockProvider()
        {
            ImageLoader.settings.webRequestProvider = new DefaultWebRequestProvider();
        }
    }
}