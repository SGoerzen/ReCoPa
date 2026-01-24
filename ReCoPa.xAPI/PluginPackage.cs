using ReCoPa.Plugins;

namespace ReCoPa.xAPI;

public class PluginPackage : IPluginPackage
{
    public string Id => "com.rwth.recopa.xapi";
    public string Name => "ReCoPa.xAPI";

    public Contributor[] Contributors =>
    [
        new Contributor
            { Name = "Sergej Görzen", Github = "https://github.com/SGoerzen", Email = "goerzen@cs.rwth-aachen.de" }
    ];
    public string Description => "Plugin enabling xAPI and LRS.";
    public IPluginComponent[] Components => [new IXApiPreview()];
}