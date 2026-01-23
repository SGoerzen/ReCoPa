namespace ReCoPa.Network;

public sealed class SocketServerOptions
{
    public int SendTimeoutMs = 5000;
    public int ReceiveTimeoutMs = 30000;

    public bool NoDelay = true;
    public bool KeepAlive = true;

    public int MaxMessageBytes = 1024 * 1024;
}