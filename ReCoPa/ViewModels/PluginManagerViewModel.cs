using System;
using System.Collections.ObjectModel;
using ReCoPa.PluginHost;

namespace ReCoPa.ViewModels;

public class PluginManagerViewModel
{
    public ObservableCollection<PluginItemViewModel> Plugins { get; } = new();

    public PluginManagerViewModel()
    {
        var loader = new PluginLoader("Plugins");
        Console.WriteLine("Loading plugins...");
        var plugins = loader.LoadPlugins();
        Console.WriteLine($"Loaded {plugins.Count} plugins.");
        foreach (var plugin in plugins)
            Plugins.Add(new PluginItemViewModel(plugin));
    }
}