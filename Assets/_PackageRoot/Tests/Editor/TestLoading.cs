using System;
using UnityEngine;
using NUnit.Framework;
using Cysharp.Threading.Tasks;
using UnityEngine.TestTools;
using System.Collections;
using System.Security.Policy;

namespace Extensions.Unity.ImageLoader.Tests
{
    public class TestLoading
    {
        const string ImageURL_A = "https://i.imgur.com/1.jpg";
        const string ImageURL_B = "https://i.imgur.com/1.jpg";
        const string ImageURL_C = "https://i.imgur.com/1.jpg";


        public async UniTask LoadSprite(string url)
        {
            var sprite = await ImageLoader.LoadSprite(url);
            Assert.AreNotEqual(sprite, null);
        }

        [UnityTest] public IEnumerator LoadSprites()
        {
            yield return LoadSprite(ImageURL_A).ToCoroutine();
            yield return LoadSprite(ImageURL_B).ToCoroutine();
            yield return LoadSprite(ImageURL_C).ToCoroutine();
        }
    }
}