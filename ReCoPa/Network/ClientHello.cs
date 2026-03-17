using System;
using System.Collections.Generic;

namespace ReCoPa.Network;

public sealed class ClientHello
{
    public string? SessionId { get; set; }
    public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> Meta { get; } = new(StringComparer.OrdinalIgnoreCase);
}