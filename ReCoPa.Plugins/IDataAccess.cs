using System;

namespace ReCoPa.Plugins;

public interface IDataAccess
{
    IDataStore Store { get; }
    IDisposable Subscribe(Action<DataPacket> handler);
}
