using System;
using System.IO;
using System.Linq;

namespace ReCoPa.Plugins;

public static class PluginPaths
{
    public static string GetAppDataDirectory()
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

        return Path.Combine(basePath, "ReCoPa");
    }

    public static string GetPluginDirectory()
    {
        return Path.Combine(GetAppDataDirectory(), "Plugins");
    }

    public static string GetPluginStateFile()
    {
        return Path.Combine(GetAppDataDirectory(), "plugin-state.json");
    }

    public static string GetPluginDataDirectory(string folderName)
    {
        if (string.IsNullOrWhiteSpace(folderName))
            folderName = "Plugin";

        var safe = SanitizeFolderName(folderName);
        return Path.Combine(GetPluginDirectory(), safe);
    }

    private static string SanitizeFolderName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Select(c => invalid.Contains(c) ? '-' : c).ToArray());
        cleaned = cleaned.Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? "Plugin" : cleaned;
    }
}
