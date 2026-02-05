using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Newtonsoft.Json;

namespace ReCoPa.Network
{
    /// <summary>
    /// One-file server host: listener + clients + routing + On&lt;T&gt;/Broadcast&lt;T&gt;.
    /// Usable as a MAUI singleton service.
    /// </summary>
    public sealed class SocketServerHost : IAsyncDisposable
    {
        private readonly SocketServerOptions _opt;
        private readonly EventRouter _router = new();
        private readonly ConcurrentDictionary<string, string> _last = new(StringComparer.Ordinal);
        
        private readonly List<IDisposable> _subs = new();
        private readonly object _subsGate = new();

        private readonly List<SocketConnection> _clients = new();
        private readonly object _gate = new();

        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private Task? _acceptTask;
        
        private readonly Action<Action>? _uiPost;

        public bool IsRunning { get; private set; }
        public int Port { get; private set; }

        public event Action<SocketConnection>? ClientConnected;
        public event Action<SocketConnection>? ClientDisconnected;
        public event Action<string, string>? EventReceived; // eventName,payload
        public event Action<Exception>? Error;

        public SocketServerHost(SocketServerOptions? options = null, Action<Action>? uiPost = null)
        {
            _opt = options ?? new SocketServerOptions();
            _uiPost = uiPost;
            
            // Built-in: accept "clients:hello" to store headers into connection.ClientHeaders
            _router.On("clients:hello", ctx =>
            {
                TryReadHeaders(ctx.Payload, ctx.Connection.ClientHeaders);
            });

            // Built-in: respond to heartbeat from clients
            _router.On("heartbeat:pong", async ctx =>
            {
                try
                {
                    var payload = JsonConvert.SerializeObject(new { ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() });
                    await ctx.Connection.EmitAsync("heartbeat:ping", payload).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Error?.Invoke(ex);
                }
            });
        }

        // ---------------------------
        // Start / Stop
        // ---------------------------

        public Task StartAsync(int port = 4567, IPAddress? ip = null, CancellationToken ct = default)
        {
            if (IsRunning) return Task.CompletedTask;

            Port = port;
            ip ??= IPAddress.Loopback; // 🔥 empfehlenswert statt Any (weniger Konflikte)

            try
            {
                _listener = new TcpListener(new IPEndPoint(ip, port));

                // ✅ MUST be set BEFORE Start()
                _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _listener.Server.ExclusiveAddressUse = false;

                _listener.Start();

                _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                _acceptTask = Task.Run(() => AcceptLoopAsync(_cts.Token));

                IsRunning = true;
                return Task.CompletedTask;
            }
            catch
            {
                try { _listener?.Stop(); } catch { }
                _listener = null;
                IsRunning = false;
                throw;
            }
        }

        public async Task StopAsync()
        {
            if (!IsRunning) return;

            _cts?.Cancel();
            try { _listener?.Stop(); } catch { /* ignore */ }

            if (_acceptTask != null)
            {
                try { await _acceptTask.ConfigureAwait(false); } catch { /* ignore */ }
            }

            List<SocketConnection> copy;
            lock (_gate) copy = new List<SocketConnection>(_clients);

            foreach (var c in copy)
            {
                try { await c.DisconnectAsync().ConfigureAwait(false); } catch { /* ignore */ }
            }

            lock (_gate) _clients.Clear();
            
            // dispose all registered handlers
            lock (_subsGate)
            {
                foreach (var s in _subs) s.Dispose();
                _subs.Clear();
            }

            _listener = null;
            _acceptTask = null;

            _cts?.Dispose();
            _cts = null;

            IsRunning = false;
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync().ConfigureAwait(false);
        }

        // ---------------------------
        // Public API
        // ---------------------------

        /// <summary>Last payload seen for an event name.</summary>
        public string? GetLast(string eventName) => _last.TryGetValue(eventName, out var v) ? v : null;

        /// <summary>
        /// Raw subscription. Works before or after StartAsync.
        /// Returns IDisposable for real unsubscription.
        /// </summary>
        public IDisposable On(string eventName, Action<string> handler, bool marshalToUiThread = true)
        {
            var sub = _router.On(eventName, ctx =>
            {
                CacheAndNotify(ctx.EventName, ctx.Payload);
                
                if (marshalToUiThread && _uiPost != null)
                    _uiPost(() => handler(ctx.Payload));
                else
                    handler(ctx.Payload);
                
                return Task.CompletedTask;
            });

            lock (_subsGate) _subs.Add(sub);
            return sub; // caller kann es ignorieren
        }

        /// <summary>
        /// Typed subscription. Payload auto-deserializes to T (Newtonsoft).
        /// If T is string => raw payload.
        /// </summary>
        public IDisposable On<T>(string eventName, Action<T> handler, bool marshalToUiThread = true)
        {
            return On(eventName, payload =>
            {
                if (typeof(T) == typeof(string))
                {
                    handler((T)(object)payload);
                    return;
                }

                var obj = JsonConvert.DeserializeObject<T>(payload);
                if (obj == null)
                    throw new JsonSerializationException($"Cannot deserialize event '{eventName}' to {typeof(T).Name}. Payload: {payload}");

                handler(obj);
            }, marshalToUiThread);
        }

        /// <summary>Broadcast raw payload to all connected clients.</summary>
        public async Task BroadcastAsync(string eventName, string payload = "", CancellationToken ct = default)
        {
            List<SocketConnection> copy;
            lock (_gate) copy = new List<SocketConnection>(_clients);
            
            Console.WriteLine($"Broadcast {eventName} to {copy.Count} clients");

            foreach (var c in copy)
            {
                if (!c.IsConnected) continue;
                try { await c.EmitAsync(eventName, payload, ct).ConfigureAwait(false); }
                catch (Exception ex) { Error?.Invoke(ex); }
            }
        }

        /// <summary>
        /// Broadcast typed payload. Auto-serializes T using Newtonsoft.
        /// If T is string => sent as-is.
        /// </summary>
        public Task BroadcastAsync<T>(string eventName, T data, CancellationToken ct = default)
        {
            string payload = data is string s ? s : JsonConvert.SerializeObject(data);
            return BroadcastAsync(eventName, payload, ct);
        }

        public Task EmitToClientAsync(Guid clientId, string eventName, string payload = "", CancellationToken ct = default)
        {
            SocketConnection? client = null;
            lock (_gate)
                client = _clients.Find(c => c.Id == clientId);

            if (client == null || !client.IsConnected)
                return Task.CompletedTask;

            return client.EmitAsync(eventName, payload, ct);
        }

        // ---------------------------
        // Accept loop & dispatch
        // ---------------------------

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

                        void invoke() => ClientDisconnected?.Invoke(c);
                        if (_uiPost != null) _uiPost(invoke);
                        else invoke();
                    };

                    lock (_gate) _clients.Add(conn);
                    {
                        void invoke() => ClientConnected?.Invoke(conn);
                        if (_uiPost != null) _uiPost(invoke);
                        else invoke();
                    }

                    conn.Start(async ctx =>
                    {
                        try
                        {
                            CacheAndNotify(ctx.EventName, ctx.Payload);
                            await _router.DispatchAsync(ctx, ex => Error?.Invoke(ex)).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Error?.Invoke(ex);
                        }
                    });
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { Error?.Invoke(ex); }
        }

        private void CacheAndNotify(string eventName, string payload)
        {
            _last[eventName] = payload;
            EventReceived?.Invoke(eventName, payload);
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
                    target[prop.Name] = prop.Value.ValueKind == JsonValueKind.String
                        ? (prop.Value.GetString() ?? "")
                        : prop.Value.ToString();
                }
            }
            catch { /* ignore malformed hello */ }
        }

        // ============================================================
        // Supporting types (kept in same file for convenience)
        // ============================================================

        public sealed class SocketServerOptions
        {
            public int MaxMessageBytes { get; set; } = 1024 * 1024;
            public int ReceiveTimeoutMs { get; set; } = 30_000;
            public int SendTimeoutMs { get; set; } = 5_000;

            public TimeSpan ReceiveTimeout => ReceiveTimeoutMs <= 0 ? TimeSpan.Zero : TimeSpan.FromMilliseconds(ReceiveTimeoutMs);
            public TimeSpan SendTimeout => SendTimeoutMs <= 0 ? TimeSpan.Zero : TimeSpan.FromMilliseconds(SendTimeoutMs);
        }

        public sealed class SocketEventContext
        {
            public SocketConnection Connection { get; }
            public string EventName { get; }
            public string Payload { get; }

            public SocketEventContext(SocketConnection connection, string eventName, string payload)
            {
                Connection = connection;
                EventName = eventName;
                Payload = payload;
            }
        }

        public sealed class SocketConnection
        {
            private readonly TcpClient _tcp;
            private readonly SocketServerOptions _opt;
            private readonly NetworkStream _stream;

            private readonly CancellationTokenSource _cts = new();
            private Task? _rxTask;

            public Guid Id { get; } = Guid.NewGuid();
            public EndPoint? RemoteEndPoint => _tcp.Client.RemoteEndPoint;
            public bool IsConnected => _tcp.Connected;
            public Dictionary<string, string> ClientHeaders { get; } = new(StringComparer.OrdinalIgnoreCase);

            public event Action<SocketConnection>? Disconnected;
            public event Action<Exception>? Error;

            public SocketConnection(TcpClient tcp, SocketServerOptions opt)
            {
                _tcp = tcp;
                _opt = opt;
                _stream = tcp.GetStream();
            }

            public void Start(Func<SocketEventContext, Task> onMessage)
            {
                _rxTask = Task.Run(async () =>
                {
                    try
                    {
                        while (!_cts.IsCancellationRequested && _tcp.Connected)
                        {
                            var (ev, payload) = await Framing.ReadMessageAsync(
                                _stream, _opt.MaxMessageBytes, _opt.ReceiveTimeout, _cts.Token).ConfigureAwait(false);

                            await onMessage(new SocketEventContext(this, ev, payload)).ConfigureAwait(false);
                        }
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        Error?.Invoke(ex);
                    }
                    finally
                    {
                        Disconnected?.Invoke(this);
                    }
                });
            }

            public Task EmitAsync(string eventName, string payload, CancellationToken ct = default)
            {
                return Framing.WriteMessageAsync(
                    _stream, eventName, payload,
                    _opt.MaxMessageBytes, _opt.SendTimeout, ct);
            }

            public Task EmitAsync<T>(string eventName, T data, CancellationToken ct = default)
            {
                string payload = data is string s ? s : JsonConvert.SerializeObject(data);
                return EmitAsync(eventName, payload, ct);
            }

            public async Task DisconnectAsync()
            {
                _cts.Cancel();
                try { _tcp.Close(); } catch { /* ignore */ }

                if (_rxTask != null)
                {
                    try { await _rxTask.ConfigureAwait(false); } catch { /* ignore */ }
                }
            }
        }

        internal sealed class EventRouter
        {
            private readonly object _gate = new();
            private readonly Dictionary<string, List<Func<SocketEventContext, Task>>> _handlers =
                new(StringComparer.Ordinal);

            public IDisposable On(string eventName, Func<SocketEventContext, Task> handler)
            {
                if (eventName == null) throw new ArgumentNullException(nameof(eventName));
                if (handler == null) throw new ArgumentNullException(nameof(handler));

                lock (_gate)
                {
                    if (!_handlers.TryGetValue(eventName, out var list))
                    {
                        list = new List<Func<SocketEventContext, Task>>();
                        _handlers[eventName] = list;
                    }
                    list.Add(handler);
                }

                return new DisposableAction(() =>
                {
                    lock (_gate)
                    {
                        if (_handlers.TryGetValue(eventName, out var list))
                            list.Remove(handler);
                    }
                });
            }

            public IDisposable On(string eventName, Action<SocketEventContext> handler)
            {
                Func<SocketEventContext, Task> wrapped = ctx =>
                {
                    handler(ctx);
                    return Task.CompletedTask;
                };
                return On(eventName, wrapped);
            }

            public async Task DispatchAsync(SocketEventContext ctx, Action<Exception>? onError)
            {
                List<Func<SocketEventContext, Task>>? copy = null;
                lock (_gate)
                {
                    if (_handlers.TryGetValue(ctx.EventName, out var list))
                        copy = new List<Func<SocketEventContext, Task>>(list);
                }

                if (copy == null) return;

                foreach (var h in copy)
                {
                    try { await h(ctx).ConfigureAwait(false); }
                    catch (Exception ex) { onError?.Invoke(ex); }
                }
            }

            private sealed class DisposableAction : IDisposable
            {
                private Action? _a;
                public DisposableAction(Action a) => _a = a;
                public void Dispose() => Interlocked.Exchange(ref _a, null)?.Invoke();
            }
        }

        internal static class Framing
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
