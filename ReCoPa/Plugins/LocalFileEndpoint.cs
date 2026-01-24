namespace ReCoPa.Plugins;

public class LocalFileEndpoint : IEndpoint
{
    public virtual string Name => "Local File Endpoint (JSONL)";
    public virtual string EndpointReference => "OmiLAXR.Endpoints.LocalFileEndpoint";
}