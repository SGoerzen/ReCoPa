namespace ReCoPa.Plugins;

public class CsvFileEndpoint : LocalFileEndpoint
{
    public override string Id => "ReCoPa.Plugins.CsvFileEndpoint";
    public override string Name => "CSV File Endpoint";
    public override string Version => "1.0.0";
    public override Contributor[] Contributors =>
    [
        new Contributor() { Name = "Sergej Görzen", Github = "https://github.com/SGoerzen", Email = "goerzen@cs.rwth-aachen.de" }
    ];
    public override string Description => "Endpoint for enabling CSV files.";
}