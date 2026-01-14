using System.Collections.Concurrent;
using System.Net;
using Newtonsoft.Json;
using ReCoPa.Network;

namespace ReCoPa.Services;

public sealed class SocketServerService : IAsyncDisposable
{
    private readonly IDispatcher _dispatcher;

    private SocketServer? _server;
    private readonly ConcurrentDictionary<string, string> _lastPayloadByEvent = new(StringComparer.Ordinal);

    public bool IsRunning { get; private set; }
    public int Port { get; private set; }

    public event Action<string, string>? EventReceived; // (eventName, payload)

    public SocketServerService(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task StartAsync(int port = 4567, CancellationToken ct = default)
    {
        if (IsRunning) return;

        Port = port;

        _server = new SocketServer(new SocketServerOptions
        {
            ReceiveTimeoutMs = 30_000,
            SendTimeoutMs = 5_000,
            MaxMessageBytes = 1024 * 1024
        });

        // Catch-all approach: register handlers for all relevant events explicitly.
        // (TCP protocol doesn't have a "OnAny" in this server variant)
        RegisterDefaultHandlers(_server);

        await _server.StartAsync(IPAddress.Any, port, ct).ConfigureAwait(false);
        IsRunning = true;
    }

    public async Task StopAsync()
    {
        if (!IsRunning || _server == null) return;

        await _server.StopAsync().ConfigureAwait(false);
        _server = null;
        IsRunning = false;
    }

    public string? GetLast(string eventName)
    {
        return _lastPayloadByEvent.TryGetValue(eventName, out var v) ? v : null;
    }

    /// Subscribe raw payload (string). By default marshals to UI thread.
    public IDisposable On(string eventName, Action<string> handler, bool marshalToUiThread = true)
    {
        if (_server == null) throw new InvalidOperationException("Server not started.");

        void Wrapped(string payload)
        {
            if (!marshalToUiThread)
            {
                handler(payload);
                return;
            }

            _dispatcher.Dispatch(() => handler(payload));
        }

        // We don't have unsubscription in SocketServer.On(...) (simple version),
        // so we provide a "soft" unsubscribe handle by ignoring after dispose.
        var alive = true;

        _server.On(eventName, ctx =>
        {
            _lastPayloadByEvent[eventName] = ctx.Payload;
            EventReceived?.Invoke(eventName, ctx.Payload);

            if (alive) Wrapped(ctx.Payload);
        });

        return new DisposableAction(() => alive = false);
    }

    /// Subscribe typed JSON payload (Newtonsoft). By default marshals to UI thread.
    public IDisposable OnJson<T>(string eventName, Action<T> handler, bool marshalToUiThread = true)
    {
        return On(eventName, payload =>
        {
            var obj = JsonConvert.DeserializeObject<T>(payload);
            if (obj != null) handler(obj);
        }, marshalToUiThread);
    }

    private void RegisterDefaultHandlers(SocketServer server)
    {
        // register the events you expect from Unity/ReCoPa:
        string[] knownEvents =
        {
            "clients:hello",
            "clients:meta",
            "clients:scenario",
            "clients:tracking",
            "clients:tracking:start",
            "clients:tracking:stop",
            "clients:tracking:pause",
            "clients:tracking:resume",
            "clients:calibration:start",
            "clients:calibration:stop",
            "clients:all",
            "clients:quit"
        };

        foreach (var ev in knownEvents)
        {
            server.On(ev, ctx =>
            {
                _lastPayloadByEvent[ev] = ctx.Payload;
                EventReceived?.Invoke(ev, ctx.Payload);
            });
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
    }

    private sealed class DisposableAction : IDisposable
    {
        private Action? _a;
        public DisposableAction(Action a) => _a = a;
        public void Dispose() => Interlocked.Exchange(ref _a, null)?.Invoke();
    }
}