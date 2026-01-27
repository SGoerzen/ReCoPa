using ReCoPa.Plugins.Endpoints;
using ReCoPa.Plugins.Visualizations;

namespace ReCoPa.Plugins;

public sealed class CorePluginPackage : IPluginPackage, ICorePlugin
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
        new CsvFileEndpoint(),
        
        new HelloWorld(),
        new PulseMonitor()
    ];
}