using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T>
    {
        internal Future<T> SetWebRequest(UnityWebRequest webRequest)
        {
            WebRequest = PrepareWebRequest(webRequest);
            return this;
        }
        protected virtual UnityWebRequest PrepareWebRequest(UnityWebRequest webRequest)
        {
            webRequest.timeout = (int)Math.Ceiling(timeout.TotalSeconds);
            return webRequest;
        }

        internal UnityWebRequestAsyncOperation SendWebRequest()
        {
            if (WebRequest == null)
                throw new Exception($"[ImageLoader] Future[id={Id}] UnityWebRequest is not set. Use SetWebRequest method before calling SendWebRequest.");

            if (LogLevel.IsActive(DebugLevel.Trace))
                Debug.Log($"[ImageLoader] Future[id={Id}] Send UnityWebRequest for loading from Source\n{Url}");

            return WebRequest.SendWebRequest();
        }

        protected abstract T ParseWebRequest(UnityWebRequest webRequest);
        protected abstract UnityWebRequest CreateWebRequest(string url);
    }
}
