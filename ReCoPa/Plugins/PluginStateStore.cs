using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ReCoPa.ViewModels;

public sealed class PluginStateStore
{
    private readonly string _path;
    private Dictionary<string, bool> _enabledById = new(StringComparer.OrdinalIgnoreCase);

    public PluginStateStore(string pluginDirectory)
    {
        Directory.CreateDirectory(pluginDirectory);
        _path = Path.Combine(pluginDirectory, "plugin-state.json");
        Load();
    }

    public bool GetEnabled(string pluginId, bool defaultValue = true)
        => _enabledById.TryGetValue(pluginId, out var v) ? v : defaultValue;

    public void SetEnabled(string pluginId, bool enabled)
    {
        _enabledById[pluginId] = enabled;
        Save();
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(_path)) return;
            var json = File.ReadAllText(_path);
            var data = JsonSerializer.Deserialize<Dictionary<string, bool>>(json);
            if (data != null) _enabledById = new Dictionary<string, bool>(data, StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            // ignore corrupt state file
            _enabledById = new(StringComparer.OrdinalIgnoreCase);
        }
    }

    private void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(_enabledById, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_path, json);
        }
        catch
        {
            // ignore write errors
        }
    }
}