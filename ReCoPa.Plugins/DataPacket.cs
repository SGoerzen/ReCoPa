using System;

namespace ReCoPa.Plugins;

public sealed record DataPacket(
    string EventName,
    string Payload,
    DateTime TimestampUtc
);
