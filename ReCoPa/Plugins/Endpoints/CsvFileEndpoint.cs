namespace ReCoPa.Plugins.Endpoints;

public class CsvFileEndpoint : LocalFileEndpoint
{
    public override string Name => "CSV File Endpoint";
    public override string EndpointReference => "OmiLAXR.Endpoints.CsvFileEndpoint";
}