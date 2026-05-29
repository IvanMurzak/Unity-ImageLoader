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
    /// image-loading tests. Serves pre-generated PNG bytes for success routes, a
    /// deliberately slow route so the client-side Future timeout fires predictably,
    /// and a deterministically *held* route whose response is blocked until the test
    /// explicitly releases it.
    ///
    /// This replaces the public image URLs the tests used to fetch from GitHub, which
    /// were rate-limited and unreliable on CI, while keeping the loader's real
    /// UnityWebRequest send/decode path intact.
    ///
    /// The held route (<c>/hold/{id}</c>) exists to make "cancel-while-loading"
    /// scenarios deterministic: the request reaches the server, the server begins a
    /// response but parks the worker thread on a per-id gate, so the client-side
    /// Future is guaranteed to be in the <c>LoadingFromSource</c> state (still
    /// in-flight, not yet memory-cached) until the test calls <see cref="ReleaseHeld"/>.
    /// This removes the timing race where a fast local load could complete before the
    /// test got a chance to cancel it.
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
        // Per-id gates for the /hold/{id} route. A held request parks on its event
        // until the test releases it (or Stop() releases everything). Created lazily
        // by HoldImage so a held route always serves a valid, registered image.
        readonly Dictionary<string, ManualResetEventSlim> holdGates = new Dictionary<string, ManualResetEventSlim>();
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

        /// <summary>
        /// Arms the held gate for <paramref name="id"/>. After this call any request to
        /// <c>/hold/{id}</c> blocks (the connection stays open, response withheld) until
        /// <see cref="ReleaseHeld"/> is called for the same id. Idempotent: re-holding an
        /// already-armed id re-blocks it. The id must also be registered via
        /// <see cref="RegisterImage"/> so a valid image is served once released.
        /// </summary>
        public void HoldImage(string id)
        {
            lock (gate)
            {
                if (holdGates.TryGetValue(id, out var existing))
                    existing.Reset();
                else
                    holdGates[id] = new ManualResetEventSlim(initialState: false);
            }
        }

        /// <summary>
        /// Releases a previously held id, letting its parked request(s) send the image
        /// and complete. Safe to call when nothing is held (no-op).
        /// </summary>
        public void ReleaseHeld(string id)
        {
            ManualResetEventSlim ev;
            lock (gate) holdGates.TryGetValue(id, out ev);
            ev?.Set();
        }

        /// <summary>
        /// Releases every held id. Used by <see cref="Stop"/> and as a safety net in
        /// test teardown so no worker thread is left parked.
        /// </summary>
        public void ReleaseAllHeld()
        {
            List<ManualResetEventSlim> all;
            lock (gate) all = new List<ManualResetEventSlim>(holdGates.Values);
            foreach (var ev in all)
                ev.Set();
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
            // Unblock any parked /hold workers so they exit cleanly and threads don't leak.
            ReleaseAllHeld();
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
                        WriteAndClose(client, stream, BuildResponse(200, "OK", "text/plain", Encoding.ASCII.GetBytes("slow")));
                        return;
                    }

                    if (path.StartsWith("/hold/"))
                    {
                        var id = path.Substring("/hold/".Length);
                        ManualResetEventSlim ev;
                        byte[] png;
                        bool found;
                        lock (gate)
                        {
                            holdGates.TryGetValue(id, out ev);
                            found = images.TryGetValue(id, out png);
                        }
                        // Park the worker until the test releases this id (or Stop()
                        // releases everything). The client request is fully sent and the
                        // connection is open, so the client-side Future is guaranteed to
                        // be in the LoadingFromSource state for the whole wait.
                        ev?.Wait();
                        if (found && running)
                        {
                            WriteAndClose(client, stream, BuildResponse(200, "OK", "image/png", png));
                            return;
                        }
                        // Released by Stop() (server shutting down) or id not registered:
                        // just drop the connection so the client errors out cleanly.
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
                            WriteAndClose(client, stream, BuildResponse(200, "OK", "image/png", png));
                            return;
                        }
                    }

                    WriteAndClose(client, stream, BuildResponse(404, "Not Found", "text/plain", Encoding.ASCII.GetBytes("not found")));
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

        // Writes the full response, then closes the connection *gracefully* to avoid a
        // TCP RST. The original implementation wrote the bytes and let `using` dispose
        // the socket immediately; on Windows, closing a socket while the peer's request
        // bytes (headers/body) are still sitting unread in the receive buffer makes the
        // OS send an RST instead of a FIN. curl then reports "Curl error 56: Recv
        // failure: Connection was reset" at random, which surfaced as a flaky
        // "Failed to receive data" test failure. The fix: drain the rest of the request,
        // half-close our send side (sends FIN), then wait for the peer's FIN before
        // disposing. This is the textbook lingering-close handshake.
        static void WriteAndClose(TcpClient client, NetworkStream stream, byte[] data)
        {
            try
            {
                stream.Write(data, 0, data.Length);
                stream.Flush();

                var socket = client.Client;
                // Drain whatever the client already sent (the rest of the HTTP request)
                // so there is no unread inbound data when we close.
                DrainAvailable(socket);
                // Send FIN on our side; the client reads EOF and closes its side.
                try { socket.Shutdown(SocketShutdown.Send); } catch { /* already closed */ }
                // Wait for the client's FIN (ReadByte returns -1) so the close is fully
                // graceful before the socket is disposed. Bounded so we never hang.
                WaitForPeerClose(stream);
            }
            catch { /* client gone */ }
        }

        static void DrainAvailable(Socket socket)
        {
            try
            {
                var buffer = new byte[1024];
                while (socket.Available > 0 && socket.Receive(buffer, 0, buffer.Length, SocketFlags.None) > 0) { }
            }
            catch { /* nothing more to drain */ }
        }

        static void WaitForPeerClose(NetworkStream stream)
        {
            try
            {
                stream.ReadTimeout = 2000;
                int b;
                do { b = stream.ReadByte(); } while (b != -1);
            }
            catch { /* timeout or reset — close anyway */ }
        }
    }
}
