using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ReCoPa.Network
{
    public sealed class SocketServer : IAsyncDisposable
    {
        private readonly SocketServerOptions _opt;
        private readonly EventRouter _router = new();
        private readonly List<SocketConnection> _clients = new();
        private readonly object _gate = new();

        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private Task? _acceptTask;

        public event Action<SocketConnection>? ClientConnected;
        public event Action<SocketConnection>? ClientDisconnected;
        public event Action<Exception>? Error;

        public SocketServer(SocketServerOptions? options = null)
        {
            _opt = options ?? new SocketServerOptions();

            // Built-in: accept "clients:hello" to store headers
            _router.On("clients:hello", ctx =>
            {
                TryReadHeaders(ctx.Payload, ctx.Connection.ClientHeaders);
            });
        }

        public void On(string eventName, Func<SocketEventContext, Task> handler) => _router.On(eventName, handler);
        public void On(string eventName, Action<SocketEventContext> handler) => _router.On(eventName, handler);

        public Task StartAsync(IPAddress ip, int port, CancellationToken ct = default)
        {
            if (_listener != null) throw new InvalidOperationException("Server already started.");

            _listener = new TcpListener(ip, port);
            _listener.Start();

            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _acceptTask = Task.Run(() => AcceptLoopAsync(_cts.Token));

            return Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            if (_cts == null) return;

            _cts.Cancel();
            try { _listener?.Stop(); } catch { }

            if (_acceptTask != null)
            {
                try { await _acceptTask.ConfigureAwait(false); } catch { }
            }

            List<SocketConnection> copy;
            lock (_gate) copy = new List<SocketConnection>(_clients);

            foreach (var c in copy)
            {
                try { await c.DisconnectAsync().ConfigureAwait(false); } catch { }
            }

            lock (_gate) _clients.Clear();

            _listener = null;
            _acceptTask = null;

            _cts.Dispose();
            _cts = null;
        }

        public async Task BroadcastAsync(string eventName, string payload, CancellationToken ct = default)
        {
            List<SocketConnection> copy;
            lock (_gate) copy = new List<SocketConnection>(_clients);

            foreach (var c in copy)
            {
                if (!c.IsConnected) continue;
                try { await c.EmitAsync(eventName, payload, ct).ConfigureAwait(false); }
                catch (Exception ex) { Error?.Invoke(ex); }
            }
        }

        private async Task AcceptLoopAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested && _listener != null)
                {
                    var tcp = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);

                    var conn = new SocketConnection(tcp, _opt);

                    conn.Error += ex => Error?.Invoke(ex);
                    conn.Disconnected += c =>
                    {
                        lock (_gate) _clients.Remove(c);
                        ClientDisconnected?.Invoke(c);
                    };

                    lock (_gate) _clients.Add(conn);
                    ClientConnected?.Invoke(conn);

                    conn.Start(async ctx =>
                    {
                        await _router.DispatchAsync(ctx, ex => Error?.Invoke(ex)).ConfigureAwait(false);
                    });
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Error?.Invoke(ex);
            }
        }

        private static void TryReadHeaders(string payload, Dictionary<string, string> target)
        {
            if (string.IsNullOrWhiteSpace(payload)) return;

            try
            {
                using var doc = JsonDocument.Parse(payload);
                if (!doc.RootElement.TryGetProperty("headers", out var headersEl)) return;
                if (headersEl.ValueKind != JsonValueKind.Object) return;

                foreach (var prop in headersEl.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.String)
                        target[prop.Name] = prop.Value.GetString() ?? "";
                    else
                        target[prop.Name] = prop.Value.ToString();
                }
            }
            catch
            {
                // ignore malformed hello
            }
        }

        public async ValueTask DisposeAsync() => await StopAsync().ConfigureAwait(false);

        // ------------------------------------------------------------
        // Framing (same as client)
        // ------------------------------------------------------------
        private static class Framing
        {
            public static async Task WriteMessageAsync(
                NetworkStream stream,
                string eventName,
                string payload,
                int maxMessageBytes,
                TimeSpan sendTimeout,
                CancellationToken ct)
            {
                if (stream == null) throw new ArgumentNullException(nameof(stream));
                if (eventName == null) throw new ArgumentNullException(nameof(eventName));
                payload ??= string.Empty;

                var evBytes = Encoding.UTF8.GetBytes(eventName);
                if (evBytes.Length > ushort.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(eventName), "eventName too long.");

                var plBytes = Encoding.UTF8.GetBytes(payload);

                int bodyLen = 2 + evBytes.Length + plBytes.Length;
                if (bodyLen <= 0 || bodyLen > maxMessageBytes)
                    throw new InvalidDataException($"Message too large: {bodyLen} > {maxMessageBytes}");

                byte[] frame = new byte[4 + bodyLen];
                BinaryPrimitives.WriteInt32BigEndian(frame.AsSpan(0, 4), bodyLen);
                BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(4, 2), (ushort)evBytes.Length);

                Buffer.BlockCopy(evBytes, 0, frame, 6, evBytes.Length);
                Buffer.BlockCopy(plBytes, 0, frame, 6 + evBytes.Length, plBytes.Length);

                using var linked = CreateTimeoutCts(sendTimeout, ct);
                await stream.WriteAsync(frame, 0, frame.Length, linked.Token).ConfigureAwait(false);
                await stream.FlushAsync(linked.Token).ConfigureAwait(false);
            }

            public static async Task<(string EventName, string Payload)> ReadMessageAsync(
                NetworkStream stream,
                int maxMessageBytes,
                TimeSpan receiveTimeout,
                CancellationToken ct)
            {
                byte[] lenBuf = new byte[4];
                await ReadExactAsync(stream, lenBuf, receiveTimeout, ct).ConfigureAwait(false);

                int bodyLen = BinaryPrimitives.ReadInt32BigEndian(lenBuf.AsSpan());
                if (bodyLen <= 0 || bodyLen > maxMessageBytes)
                    throw new InvalidDataException($"Invalid body length {bodyLen} (limit {maxMessageBytes}).");

                byte[] body = new byte[bodyLen];
                await ReadExactAsync(stream, body, receiveTimeout, ct).ConfigureAwait(false);

                ushort evLen = BinaryPrimitives.ReadUInt16BigEndian(body.AsSpan(0, 2));
                if (evLen == 0 || 2 + evLen > bodyLen)
                    throw new InvalidDataException("Invalid event name length.");

                string ev = Encoding.UTF8.GetString(body, 2, evLen);
                string payload = Encoding.UTF8.GetString(body, 2 + evLen, bodyLen - (2 + evLen));
                return (ev, payload);
            }

            private static async Task ReadExactAsync(NetworkStream stream, byte[] buffer, TimeSpan timeout, CancellationToken ct)
            {
                int offset = 0;
                using var linked = CreateTimeoutCts(timeout, ct);

                while (offset < buffer.Length)
                {
                    int read = await stream.ReadAsync(buffer, offset, buffer.Length - offset, linked.Token).ConfigureAwait(false);
                    if (read == 0) throw new EndOfStreamException("Remote closed connection.");
                    offset += read;
                }
            }

            private static CancellationTokenSource CreateTimeoutCts(TimeSpan timeout, CancellationToken ct)
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                if (timeout > TimeSpan.Zero) cts.CancelAfter(timeout);
                return cts;
            }
        }
    }
}
