using System;
using UnityEngine.Networking;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T> : IFuture, IDisposable
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
            {
                throw new Exception($"[ImageLoader] Future[id={id}] UnityWebRequest is not set. Use SetWebRequest method before calling SendWebRequest.");
            }
            return WebRequest.SendWebRequest();
        }
    }
}
