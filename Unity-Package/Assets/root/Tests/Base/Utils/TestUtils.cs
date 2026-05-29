using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

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
        // Polls GC + finalizers until `condition` holds or the timeout elapses.
        // Reference release is finalizer-driven and non-deterministic (especially on
        // Mono), so a fixed number of WaitForGC calls is flaky — keep collecting until
        // the expected state is reached, then let the caller assert.
        public static IEnumerator WaitForGCUntil(Func<bool> condition, double timeoutSeconds = 10, int millisecondsDelay = 50)
        {
            var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);
            WaitForGCFast();
            while (!condition() && DateTime.UtcNow < deadline)
            {
                yield return Wait(TimeSpan.FromMilliseconds(millisecondsDelay));
                WaitForGCFast();
            }
        }
        // Skips a test when running headless (Unity -batchmode, i.e. CI). Used by tests
        // that assert finalizer/GC-driven cleanup: reference auto-release on out-of-scope
        // depends on the GC finalizer, which is non-deterministic under Unity's
        // conservative (Boehm) collector in headless CI (stale stack/register slots act
        // as false roots). These still run in the interactive Editor Test Runner.
        public static void SkipIfHeadless()
        {
            if (Application.isBatchMode)
                Assert.Ignore("Skipped in headless CI: finalizer/GC-driven reference cleanup is non-deterministic under Unity's conservative collector. Runs interactively in the Editor Test Runner.");
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
            // Safety net: release any gate left armed by a previous (possibly failed)
            // test so no server worker stays parked across tests.
            TestHttpServer.Instance.ReleaseAllHeld();

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

        // Maps a registered image URL to its server-side image id (the index used by
        // EnableMockProvider). Mirrors the registration in EnableMockProvider.
        static string ImageIdForUrl(string url)
        {
            for (int i = 0; i < ImageURLs.Length; i++)
                if (ImageURLs[i] == url)
                    return i.ToString();
            throw new ArgumentException($"URL is not a registered test image: {url}", nameof(url));
        }

        /// <summary>
        /// Makes loading <paramref name="url"/> deterministically *hold* in-flight: the
        /// request reaches the in-process server and parks there, so any Future loading
        /// this URL stays in the LoadingFromSource state until <see cref="ReleaseHeld"/>
        /// is called. This replaces racing the clock to catch a fast local load while it
        /// is still running. Must be paired with <see cref="ReleaseHeld"/>.
        /// </summary>
        public static void BeginHold(string url)
        {
            var id = ImageIdForUrl(url);
            TestHttpServer.Instance.EnsureStarted();
            TestHttpServer.Instance.HoldImage(id);
            MockWebRequestProvider.Instance.RegisterHeld(url, id);
        }

        /// <summary>
        /// Releases a hold started by <see cref="BeginHold"/>: the parked server response
        /// is sent and the URL is routed back to the normal fast route. Safe to call even
        /// if nothing is held.
        /// </summary>
        public static void ReleaseHeld(string url)
        {
            var id = ImageIdForUrl(url);
            TestHttpServer.Instance.ReleaseHeld(id);
            MockWebRequestProvider.Instance.UnregisterHeld(url);
        }
    }
}