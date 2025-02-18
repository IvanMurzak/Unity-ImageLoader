using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    public partial class FakeFuture<T>
    {
        List<EventData> events = new List<EventData>();
        List<Action> cancelEvents = new List<Action>();


        public IReadOnlyList<EventData> Events
        {
            get
            {
                lock (events)
                    return events.Select(x => x).ToList().AsReadOnly();
            }
        }

        public FakeFuture(string url, DebugLevel localLogLevel = DebugLevel.Trace)
        {
            Url = url;

            LogLevel = localLogLevel;
            if (LogLevel.IsActive(DebugLevel.Trace))
                Debug.Log($"FakeFuture[id={Id}] created");
            Subscribe();
        }

        protected void Clear()
        {
            // ignore
        }

        protected UnityWebRequest CreateWebRequest(string url) => throw new NotImplementedException();
        protected T ParseBytes(byte[] bytes) => throw new NotImplementedException();
        protected T ParseWebRequest(UnityWebRequest webRequest) => throw new NotImplementedException();
        protected void ReleaseMemory(T obj) => throw new NotImplementedException();

        public void Dispose()
        {
            Clear();
            lock (events) events.Clear();
            lock(cancelEvents) cancelEvents.Clear();
        }
    }
}