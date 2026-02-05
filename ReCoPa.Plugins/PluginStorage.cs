using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ReCoPa.Plugins;

public sealed class PluginStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public string RootPath { get; }

    public PluginStorage(string rootPath)
    {
        RootPath = rootPath;
        Directory.CreateDirectory(RootPath);
    }

    public string GetPath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            fileName = "settings.json";

        return Path.Combine(RootPath, fileName);
    }

    public string GetVisualizationPath(string visualizationName)
    {
        var safe = SanitizeFilePart(visualizationName);
        return Path.Combine(RootPath, "visualizations", $"{safe}.json");
    }

    public void Save<T>(string fileName, T data)
    {
        var path = GetPath(fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(data, JsonOptions));
    }

    public bool TryLoad<T>(string fileName, out T? data)
    {
        data = default;
        var path = GetPath(fileName);
        if (!File.Exists(path))
            return false;

        try
        {
            var json = File.ReadAllText(path);
            data = JsonSerializer.Deserialize<T>(json, JsonOptions);
            return data is not null;
        }
        catch
        {
            return false;
        }
    }

    public void SaveVisualization<T>(string visualizationName, T data)
    {
        var path = GetVisualizationPath(visualizationName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(data, JsonOptions));
    }

    public bool TryLoadVisualization<T>(string visualizationName, out T? data)
    {
        data = default;
        var path = GetVisualizationPath(visualizationName);
        if (!File.Exists(path))
            return false;

        try
        {
            var json = File.ReadAllText(path);
            data = JsonSerializer.Deserialize<T>(json, JsonOptions);
            return data is not null;
        }
        catch
        {
            return false;
        }
    }

    private static string SanitizeFilePart(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "visualization";

        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(value.Select(c => invalid.Contains(c) ? '-' : c).ToArray());
        cleaned = cleaned.Replace(' ', '-').Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? "visualization" : cleaned.ToLowerInvariant();
    }
}
