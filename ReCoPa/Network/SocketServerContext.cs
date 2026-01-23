namespace ReCoPa.Network;

public sealed class SocketEventContext
{
    public string EventName { get; }
    public string Payload { get; }
    public SocketConnection Connection { get; }

    public SocketEventContext(string eventName, string payload, SocketConnection connection)
    {
        EventName = eventName;
        Payload = payload;
        Connection = connection;
    }
}