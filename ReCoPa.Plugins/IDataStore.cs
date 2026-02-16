using System.Collections.Generic;

namespace ReCoPa.Plugins;

public interface IDataStore
{
    IReadOnlyList<DataPacket> Query(DataQuery query);
    DataPacket? Latest(string? eventName = null);
    int Count(string? eventName = null);
}
