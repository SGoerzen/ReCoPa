
using ReCoPa.Network;

namespace ReCoPa.Models;

public class XRClient
{
    private readonly SocketServerHost.SocketConnection _conn;

    public Guid Id => _conn.Id; 
    public string? RemoteEndPoint { get; set; }
    public string? ClientType { get; set; }
    public string? Version { get; set; }
    
    public bool IsConnected => _conn.IsConnected;

    
    public XRClient(SocketServerHost.SocketConnection conn)
    {
        _conn = conn;
    }
    
    public Task EmitAsync(string eventName, string payload = "", CancellationToken ct = default)
        => _conn.EmitAsync(eventName, payload, ct);
}