using System;
using System.IO;

namespace ReCoPa.Plugins;

public static class PluginPaths
{
    public static string GetPluginDirectory()
    {
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        // macOS: ~/Library/Application Support
        if (OperatingSystem.IsMacOS())
            basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                "Library", "Application Support");

        // Linux: ~/.local/share
        if (OperatingSystem.IsLinux())
            basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                ".local", "share");

        return Path.Combine(basePath, "ReCoPa", "Plugins");
    }

    public static string GetPluginStateFile()
    {
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        if (OperatingSystem.IsMacOS())
            basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                "Library", "Application Support");

        if (OperatingSystem.IsLinux())
            basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                ".local", "share");

        return Path.Combine(basePath, "ReCoPa", "plugin-state.json");
    }
}