using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ReCoPa.PluginHost;

namespace ReCoPa.Plugins;

public sealed class PluginManager
{
    private readonly List<IPluginPackage> _plugins = new();
    private readonly List<IPluginComponent> _components = new();

    public IReadOnlyList<IPluginPackage> Plugins => _plugins;
    public IReadOnlyList<IPluginComponent> Components => _components;

    public IEnumerable<IEndpoint> Endpoints => _components.OfType<IEndpoint>();

    public IEnumerable<IVisualization> Visualizations => _components.OfType<IVisualization>();

    private string? _pluginDirectory;

    public string GetPath() => _pluginDirectory!;

    public void SetPath(string path) => _pluginDirectory = path;
    
    public void Load(string path = "")
    {
        _plugins.Clear();
        if (!string.IsNullOrWhiteSpace(path)) SetPath(path);
        Console.WriteLine($"Loading plugins from {GetPath()}...");
        var pluginLoader = new PluginLoader(_pluginDirectory!);
        var plugins = pluginLoader.LoadPlugins();
        _plugins.AddRange(plugins);
        foreach (var p in plugins)
            _components.AddRange(p.Components);
        Console.WriteLine($"Loaded {_plugins.Count} plugins with {_components.Count} components.");
    }
    
    public void OpenFolderExplorer()
    {
        var folderPath = GetPath();
        if (string.IsNullOrWhiteSpace(folderPath))
            return;

        if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{folderPath}\"",
                UseShellExecute = true
            });
        }
        else if (OperatingSystem.IsMacOS())
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "open",
                Arguments = $"\"{folderPath}\"",
                UseShellExecute = false
            });
        }
        else // Linux / BSD etc.
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "xdg-open",
                Arguments = $"\"{folderPath}\"",
                UseShellExecute = false
            });
        }
    }
}