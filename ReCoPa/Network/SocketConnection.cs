using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ReCoPa.Network;

public sealed class SocketConnection : IAsyncDisposable
{
    private readonly SocketServerOptions _opt;
    private readonly TcpClient _tcp;
    private readonly NetworkStream _stream;
    private readonly CancellationTokenSource _cts = new();
    private Task? _recvTask;

    internal SocketConnection(TcpClient tcp, SocketServerOptions opt)
    {
        _tcp = tcp;
        _opt = opt;
        _stream = tcp.GetStream();

        ConfigureSocket(tcp, opt);
    }

    public Guid Id { get; } = Guid.NewGuid();
    public EndPoint? RemoteEndPoint => _tcp.Client?.RemoteEndPoint;
    public bool IsConnected => _tcp.Connected;

    // Optional: stores "ExtraHeaders" the client sent in "hello"
    public Dictionary<string, string> ClientHeaders { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public event Action<SocketConnection>? Disconnected;
    public event Action<Exception>? Error;

    internal void Start(Func<SocketEventContext, Task> dispatch)
    {
        _recvTask = Task.Run(() => ReceiveLoopAsync(dispatch, _cts.Token));
    }

    public Task EmitAsync(string eventName, string payload, CancellationToken ct = default)
    {
        return Framing.WriteMessageAsync(
            _stream,
            eventName,
            payload ?? string.Empty,
            _opt.MaxMessageBytes,
            TimeSpan.FromMilliseconds(Math.Max(0, _opt.SendTimeoutMs)),
            ct
        );
    }

    public async Task DisconnectAsync()
    {
        _cts.Cancel();
        try
        {
            _tcp.Close();
        }
        catch
        {
        }

        if (_recvTask != null)
        {
            try
            {
                await _recvTask.ConfigureAwait(false);
            }
            catch
            {
            }
        }
    }

    private async Task ReceiveLoopAsync(Func<SocketEventContext, Task> dispatch, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var (ev, payload) = await Framing.ReadMessageAsync(
                    _stream,
                    _opt.MaxMessageBytes,
                    TimeSpan.FromMilliseconds(Math.Max(0, _opt.ReceiveTimeoutMs)),
                    ct
                ).ConfigureAwait(false);

                await dispatch(new SocketEventContext(ev, payload, this)).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Error?.Invoke(ex);
        }
        finally
        {
            Disconnected?.Invoke(this);
        }
    }

    private static void ConfigureSocket(TcpClient tcp, SocketServerOptions opt)
    {
        try
        {
            tcp.NoDelay = opt.NoDelay;
            if (opt.KeepAlive) tcp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        }
        catch
        {
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync().ConfigureAwait(false);
        _cts.Dispose();
    }
}