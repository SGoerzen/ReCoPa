using ReCoPa.Plugins.Endpoints;
using ReCoPa.Plugins.Visualizations;

namespace ReCoPa.Plugins;

public sealed class CorePluginPackage : PluginPackageBase, ICorePlugin
{
    public override string Id => "com.rwth.recopa.plugins.core";
    public override string Name => "Core Plugin";
    public override Contributor[] Contributors =>
    [
        new Contributor
            { Name = "Sergej Görzen", Github = "https://github.com/SGoerzen", Email = "goerzen@cs.rwth-aachen.de" }
    ];
    public override string Description => "Delivering core endpoints and filters.";

    public override IPluginComponent[] Components => [
        new LocalFileEndpoint(),
        new CsvFileEndpoint(),
        
        new PulseMonitor(),
        new Annotation(),
    ];

    public override string Website => "https://omilaxr.dev/recopa";
    public override string Repository => "https://github.com/SGoerzen/ReCoPa";
    public override string ChangelogUrl => "https://github.com/SGoerzen/ReCoPa/blob/main/CHANGELOG.md";
}
