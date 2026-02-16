using System;

namespace ReCoPa.Plugins;

public sealed class DataQuery
{
    public string? EventName { get; init; }
    public DateTime? SinceUtc { get; init; }
    public DateTime? UntilUtc { get; init; }
    public int? Limit { get; init; }
    public bool NewestFirst { get; init; }
}
