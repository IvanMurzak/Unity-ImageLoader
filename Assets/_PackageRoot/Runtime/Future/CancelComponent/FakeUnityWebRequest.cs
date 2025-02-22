using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    public partial class FakeUnityWebRequest : UnityWebRequest, IDisposable
    {
        public FakeUnityWebRequest(string url, string method = UnityWebRequest.kHttpVerbGET)
        {
            this.url = url;
            this.method = method;
        }

        public new void Dispose()
        {
            base.Dispose();
            // ignore

        }

        public new UnityWebRequestAsyncOperation SendWebRequest()
        {
            var asyncOperation = new UnityWebRequestAsyncOperation();
            return asyncOperation;
        }

        public new void Abort()
        {
            // ignore
        }

        // public new bool isDone => true;

        // public override bool isNetworkError => false;

        // public override bool isHttpError => false;

        // public override DownloadHandler downloadHandler { get; set; }

        // public override UploadHandler uploadHandler { get; set; }

        // public override bool disposeDownloadHandlerOnDispose { get; set; }

        // public override bool disposeUploadHandlerOnDispose { get; set; }

        // public override bool isModifiable => false;

        // public override bool useHttpContinue { get; set; }

        // public override bool use100Continue { get; set; }

        // public override bool useErrorStream { get; set; }

        // public override bool useEtag { get; set; }

        // public override bool useHttpContinue => false;

        // public override bool use100Continue => false;

        // public override bool useErrorStream => false;

        // public override bool useEtag => false;

        // public override bool isModifiable => false;

        // public override bool disposeDownloadHandlerOnDispose => false;

        // public override bool disposeUploadHandlerOnDispose => false;

        // public override bool SendWebRequest()
        // {
        //     return true;
        // }

        // public override void SetRequestHeader(string name, string value)
        // {
        //     // ignore
        // }

        // public override string GetResponseHeader(string name)
        // {
        //     return null;
        // }

        // public override string GetRequestHeader(string name)
        // {
        //     return null;
        // }

        // public override Dictionary<string, string> GetResponseHeaders()
        // {
        //     return new Dictionary<string, string>();
        // }

        // public override void SetUploadHandler(UploadHandler uh)
        // {
        //     // ignore
        // }

        // public override void SetDownloadHandler(DownloadHandler dh)
        // {
        //     // ignore
        // }

        // public override void SetTimeoutMili(int timeout)
        // {
        //     // ignore
        // }

        // public override void SetRedirectLimit(int limit)
        // {
        //     // ignore
        // }

        // public override void SetChunkedTransfer(bool chunked)
        // {
        //     // ignore
        // }


        // -----------------

    }
}