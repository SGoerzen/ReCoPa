using ReCoPa.Plugins;

namespace ReCoPa.ViewModels;

public class PluginItemViewModel
{
    public string Name { get; }
    public string Version { get; }

    public PluginItemViewModel(IPlugin plugin)
    {
        Name = plugin.Name;
        Version = plugin.Version;
    }
}