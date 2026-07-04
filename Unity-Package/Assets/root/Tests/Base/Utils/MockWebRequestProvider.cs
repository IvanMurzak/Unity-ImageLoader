using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    /// <summary>
    /// Mock implementation of IWebRequestProvider for testing
    /// Provides controllable responses without actual network requests
    /// </summary>
    public class MockWebRequestProvider : IWebRequestProvider
    {
        private static MockWebRequestProvider instance;
        public static MockWebRequestProvider Instance => instance ??= new MockWebRequestProvider();

        private readonly Dictionary<string, MockResponse> responses = new Dictionary<string, MockResponse>();
        private MockResponse defaultResponse = MockResponse.CreateSuccess(TestUtils.CreateTestTexture());

        public void Reset()
        {
            responses.Clear();
            defaultResponse = MockResponse.CreateSuccess(TestUtils.CreateTestTexture());
        }

        public void SetDefaultResponse(MockResponse response)
        {
            defaultResponse = response;
        }

        public void SetResponse(string url, MockResponse response)
        {
            responses[url] = response;
        }

        public UnityWebRequest CreateTextureRequest(string url)
        {
            var response = responses.ContainsKey(url) ? responses[url] : defaultResponse;
            return new MockWebRequest(url, response, MockWebRequest.RequestType.Texture);
        }

        public UnityWebRequest CreateDataRequest(string url)
        {
            var response = responses.ContainsKey(url) ? responses[url] : defaultResponse;
            return new MockWebRequest(url, response, MockWebRequest.RequestType.Data);
        }
    }

    /// <summary>
    /// Represents a mock response configuration
    /// </summary>
    public class MockResponse
    {
        public bool IsSuccess { get; set; }
        public string Error { get; set; }
        public byte[] Data { get; set; }
        public Texture2D Texture { get; set; }
        public float DelaySeconds { get; set; }
        public bool ShouldTimeout { get; set; }

        public static MockResponse CreateSuccess(Texture2D texture, float delaySeconds = 0f)
        {
            return new MockResponse
            {
                IsSuccess = true,
                Texture = texture,
                Data = texture?.EncodeToPNG() ?? new byte[0],
                DelaySeconds = delaySeconds
            };
        }

        public static MockResponse CreateSuccess(byte[] data, float delaySeconds = 0f)
        {
            return new MockResponse
            {
                IsSuccess = true,
                Data = data,
                DelaySeconds = delaySeconds
            };
        }

        public static MockResponse CreateError(string error = "Mock network error", float delaySeconds = 0f)
        {
            return new MockResponse
            {
                IsSuccess = false,
                Error = error,
                DelaySeconds = delaySeconds
            };
        }

        public static MockResponse CreateTimeout()
        {
            return new MockResponse
            {
                ShouldTimeout = true,
                DelaySeconds = float.MaxValue
            };
        }
    }
}