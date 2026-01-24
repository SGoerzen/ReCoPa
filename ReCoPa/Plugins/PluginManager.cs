using System;
using System.Collections.Generic;
using System.Linq;
using ReCoPa.PluginHost;

namespace ReCoPa.Plugins;

public sealed class PluginManager
{
    private readonly List<IPlugin> _plugins = new();

    public IReadOnlyList<IPlugin> Plugins => _plugins;

    public IEnumerable<IEndpoint> Endpoints => _plugins.OfType<IEndpoint>();

    public IEnumerable<IVisualization> VisualizationPlugins => _plugins.OfType<IVisualization>();

    private string? _pluginDirectory;

    public string GetPath() => _pluginDirectory!;

    public void SetPath(string path) => _pluginDirectory = path;
    
    public void Load(string path = "")
    {
        _plugins.Clear();
        if (!string.IsNullOrWhiteSpace(path)) SetPath(path);
        Console.WriteLine($"Loading plugins from {GetPath()}...");
        var pluginLoader = new PluginLoader(_pluginDirectory!);
        _plugins.AddRange(pluginLoader.LoadPlugins());
        Console.WriteLine($"Loaded {_plugins.Count} plugins.");
    }
}