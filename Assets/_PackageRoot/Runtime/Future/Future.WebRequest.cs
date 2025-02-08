using System;
using UnityEngine.Networking;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T> : IFuture, IDisposable
    {
        internal Future<T> SetWebRequest(UnityWebRequest webRequest)
        {
            webRequest.timeout = (int)Math.Ceiling(timeout.TotalSeconds);
            this.WebRequest = webRequest;
            return this;
        }

        internal UnityWebRequestAsyncOperation SendWebRequest()
        {
            if (WebRequest == null)
            {
                throw new Exception("[ImageLoader] UnityWebRequest is not set. Use SetWebRequest method before calling SendWebRequest.");
            }
            return WebRequest.SendWebRequest();
        }
    }
}
