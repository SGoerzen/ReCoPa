namespace ReCoPa.Plugins;

public interface IDataConsumer
{
    void OnData(DataPacket data);
}
