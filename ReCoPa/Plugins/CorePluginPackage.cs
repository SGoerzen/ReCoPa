namespace ReCoPa.Plugins;

public class CorePluginPackage : IPluginPackage
{
    public string Id => "com.rwth.recopa.core-plugin";
    public string Name => "Core Plugin";
    public Contributor[] Contributors =>
    [
        new Contributor
            { Name = "Sergej Görzen", Github = "https://github.com/SGoerzen", Email = "goerzen@cs.rwth-aachen.de" }
    ];
    public string Description => "Delivering core endpoints and filters.";

    public IPluginComponent[] Components => [
        new LocalFileEndpoint(),
        new CsvFileEndpoint()
    ];
}