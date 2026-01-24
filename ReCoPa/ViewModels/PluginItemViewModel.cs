using ReCoPa.Plugins;

namespace ReCoPa.ViewModels;

public class PluginItemViewModel
{
    public string Name { get; }
    public string Version { get; }

    public PluginItemViewModel(IPluginPackage pluginPackage)
    {
        Name = pluginPackage.Name;
        Version = pluginPackage.GetType().Assembly.GetName().Version?.ToString() ?? "0.0.0";
    }
}