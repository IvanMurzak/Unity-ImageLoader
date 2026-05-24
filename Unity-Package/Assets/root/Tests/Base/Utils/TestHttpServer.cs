using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Extensions.Unity.ImageLoader.Tests.Utils
{
    /// <summary>
    /// Minimal in-process HTTP server bound to 127.0.0.1 for deterministic, offline
    /// image-loading tests. Serves pre-generated PNG bytes for success routes and a
    /// deliberately slow route so the client-side Future timeout fires predictably.
    ///
    /// This replaces the public image URLs the tests used to fetch from GitHub, which
    /// were rate-limited and unreliable on CI, while keeping the loader's real
    /// UnityWebRequest send/decode path intact.
    /// </summary>
    public sealed class TestHttpServer
    {
        static TestHttpServer instance;
        public static TestHttpServer Instance
        {
            get
            {
                if (instance == null) instance = new TestHttpServer();
                return instance;
            }
        }

        readonly object gate = new object();
        readonly Dictionary<string, byte[]> images = new Dictionary<string, byte[]>();
        TcpListener listener;
        Thread acceptThread;
        volatile bool running;

        public int Port { get; private set; }
        public string BaseUrl => $"http://127.0.0.1:{Port}";

        /// <summary>
        /// Delay applied to the <c>/slow</c> route. Must comfortably exceed the
        /// client-side Future timeout used by failing tests (100 ms) so the timeout
        /// fires before any response arrives.
        /// </summary>
        public static readonly TimeSpan SlowDelay = TimeSpan.FromSeconds(5);

        public void EnsureStarted()
        {
            lock (gate)
            {
                if (running) return;
                listener = new TcpListener(IPAddress.Loopback, 0);
                listener.Start();
                Port = ((IPEndPoint)listener.LocalEndpoint).Port;
                running = true;
                acceptThread = new Thread(AcceptLoop) { IsBackground = true, Name = "TestHttpServer" };
                acceptThread.Start();
            }
        }

        public void RegisterImage(string id, byte[] pngBytes)
        {
            lock (gate) images[id] = pngBytes;
        }

        public void Stop()
        {
            lock (gate)
            {
                if (!running) return;
                running = false;
                try { listener?.Stop(); } catch { /* already stopped */ }
                listener = null;
            }
        }

        void AcceptLoop()
        {
            while (running)
            {
                TcpClient client;
                try { client = listener.AcceptTcpClient(); }
                catch { break; } // listener stopped
                var worker = new Thread(() => HandleClient(client)) { IsBackground = true };
                worker.Start();
            }
        }

        void HandleClient(TcpClient client)
        {
            try
            {
                using (client)
                using (var stream = client.GetStream())
                {
                    var requestLine = ReadRequestLine(stream);
                    if (string.IsNullOrEmpty(requestLine)) return;

                    // Request line shape: "GET /path HTTP/1.1"
                    var parts = requestLine.Split(' ');
                    var path = parts.Length >= 2 ? parts[1] : "/";

                    if (path == "/slow")
                    {
                        // Hold the response so the client-side timeout fires first.
                        Thread.Sleep(SlowDelay);
                        TryWrite(stream, BuildResponse(200, "OK", "text/plain", Encoding.ASCII.GetBytes("slow")));
                        return;
                    }

                    if (path.StartsWith("/img/"))
                    {
                        var id = path.Substring("/img/".Length);
                        byte[] png;
                        bool found;
                        lock (gate) found = images.TryGetValue(id, out png);
                        if (found)
                        {
                            TryWrite(stream, BuildResponse(200, "OK", "image/png", png));
                            return;
                        }
                    }

                    TryWrite(stream, BuildResponse(404, "Not Found", "text/plain", Encoding.ASCII.GetBytes("not found")));
                }
            }
            catch { /* client aborted or disconnected — expected for slow/timeout tests */ }
        }

        static string ReadRequestLine(NetworkStream stream)
        {
            var sb = new StringBuilder();
            int prev = -1, cur;
            while ((cur = stream.ReadByte()) != -1)
            {
                if (prev == '\r' && cur == '\n') break;
                if (cur != '\r') sb.Append((char)cur);
                prev = cur;
            }
            return sb.ToString();
        }

        static byte[] BuildResponse(int code, string reason, string contentType, byte[] body)
        {
            var header = $"HTTP/1.1 {code} {reason}\r\n" +
                         $"Content-Type: {contentType}\r\n" +
                         $"Content-Length: {body.Length}\r\n" +
                         "Connection: close\r\n\r\n";
            var headerBytes = Encoding.ASCII.GetBytes(header);
            var response = new byte[headerBytes.Length + body.Length];
            Buffer.BlockCopy(headerBytes, 0, response, 0, headerBytes.Length);
            Buffer.BlockCopy(body, 0, response, headerBytes.Length, body.Length);
            return response;
        }

        static void TryWrite(NetworkStream stream, byte[] data)
        {
            try { stream.Write(data, 0, data.Length); stream.Flush(); } catch { /* client gone */ }
        }
    }
}
