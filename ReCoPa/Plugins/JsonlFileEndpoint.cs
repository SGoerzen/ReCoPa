namespace ReCoPa.Plugins;

public class JsonlFileEndpoint : LocalFileEndpoint
{
    public override string Id => "ReCoPa.Plugins.JsonlFileEndpoint";
    public override string Name => "JSONL File Endpoint";
    public override string Version => "1.0.0";
    public override Contributor[] Contributors =>
    [
        new Contributor() { Name = "Sergej Görzen", Github = "https://github.com/SGoerzen", Email = "goerzen@cs.rwth-aachen.de" }
    ];
    public override string Description => "Endpoint for enabling JSONL files.";
}