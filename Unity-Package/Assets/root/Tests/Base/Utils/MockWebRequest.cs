using System;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    /// <summary>
    /// Mock UnityWebRequest that simulates network behavior without actual networking
    /// Uses composition pattern instead of inheritance to work around Unity's sealed classes
    /// </summary>
    public class MockWebRequest : UnityWebRequest
    {
        public enum RequestType
        {
            Texture,
            Data
        }

        private readonly MockResponse mockResponse;
        private readonly RequestType requestType;
        private UnityWebRequestAsyncOperation asyncOperation;
        private bool isCompleted = false;

        public MockWebRequest(string url, MockResponse response, RequestType type) : base(url)
        {
            mockResponse = response;
            requestType = type;

            // Set up appropriate download handler with reflection to avoid sealed class issues
            if (type == RequestType.Texture)
            {
                downloadHandler = new DownloadHandlerTexture(false);
                SetDownloadHandlerData();
            }
            else
            {
                downloadHandler = new DownloadHandlerBuffer();
                SetDownloadHandlerData();
            }
        }

        private void SetDownloadHandlerData()
        {
            try
            {
                // Use reflection to set the internal data
                var dataField = typeof(DownloadHandler).GetField("m_Data", BindingFlags.NonPublic | BindingFlags.Instance);
                if (dataField != null)
                {
                    dataField.SetValue(downloadHandler, mockResponse?.Data ?? new byte[0]);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Could not set download handler data via reflection: {ex.Message}");
            }
        }

        public new UnityWebRequestAsyncOperation SendWebRequest()
        {
            if (asyncOperation != null)
                return asyncOperation;

            // Create a dummy async operation that we can control
            // Use a valid URL to avoid connection errors, but we'll override the result
            var dummyRequest = new UnityWebRequest("https://httpbin.org/status/200");
            dummyRequest.downloadHandler = new DownloadHandlerBuffer();
            asyncOperation = dummyRequest.SendWebRequest();

            // Immediately abort to prevent actual network call
            dummyRequest.Abort();
            dummyRequest.Dispose();

            // Simulate the async behavior
            UniTask.Run(async () =>
            {
                try
                {
                    if (mockResponse.ShouldTimeout)
                    {
                        // For timeout, we just wait indefinitely
                        await UniTask.Delay(TimeSpan.FromHours(1));
                        return;
                    }

                    if (mockResponse.DelaySeconds > 0)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(mockResponse.DelaySeconds));
                    }

                    // Complete the operation on main thread
                    await UniTask.SwitchToMainThread();
                    CompleteOperation();
                }
                catch (OperationCanceledException)
                {
                    await UniTask.SwitchToMainThread();
                    CompleteOperation();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    await UniTask.SwitchToMainThread();
                    CompleteOperation();
                }
            }).Forget();

            return asyncOperation;
        }

        private void CompleteOperation()
        {
            if (isCompleted) return;
            isCompleted = true;

            try
            {
                if (!mockResponse.IsSuccess && !string.IsNullOrEmpty(mockResponse.Error))
                {
                    // Use reflection to set internal state
                    SetInternalError(mockResponse.Error);
                }
                else if (mockResponse.IsSuccess)
                {
                    // Set success state
                    SetInternalSuccess();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error setting mock response state: {ex.Message}");
            }
        }

        private void SetInternalError(string errorMessage)
        {
            try
            {
                // Set result to connection error
                var resultField = typeof(UnityWebRequest).GetField("m_Result", BindingFlags.NonPublic | BindingFlags.Instance);
                if (resultField != null)
                {
#if UNITY_2020_1_OR_NEWER
                    resultField.SetValue(this, UnityWebRequest.Result.ProtocolError);
#endif
                }

                // Set error message
                var errorField = typeof(UnityWebRequest).GetField("m_Error", BindingFlags.NonPublic | BindingFlags.Instance);
                if (errorField != null)
                {
                    errorField.SetValue(this, errorMessage);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to set internal error state via reflection: {ex.Message}");
            }
        }

        private void SetInternalSuccess()
        {
            try
            {
                // Set result to success
                var resultField = typeof(UnityWebRequest).GetField("m_Result", BindingFlags.NonPublic | BindingFlags.Instance);
                if (resultField != null)
                {
#if UNITY_2020_1_OR_NEWER
                    resultField.SetValue(this, UnityWebRequest.Result.Success);
#endif
                }

                // Ensure data is available in download handler
                SetDownloadHandlerData();

                // For texture requests, set the texture
                if (requestType == RequestType.Texture && downloadHandler is DownloadHandlerTexture textureHandler && mockResponse?.Texture != null)
                {
                    try
                    {
                        var textureField = typeof(DownloadHandlerTexture).GetField("m_Texture", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (textureField != null)
                        {
                            textureField.SetValue(textureHandler, mockResponse.Texture);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Could not set texture via reflection: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to set internal success state via reflection: {ex.Message}");
            }
        }
    }
}