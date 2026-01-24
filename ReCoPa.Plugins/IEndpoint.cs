namespace ReCoPa.Plugins;

public interface IEndpoint : IPluginComponent
{
    public string EndpointReference { get; }
}