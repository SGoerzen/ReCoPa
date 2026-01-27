using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;

namespace ReCoPa.ViewModels;

public sealed class PluginManagerViewModel : ViewModelBase
{
    // PASSE DAS an deinen echten Core-Plugin Id-String an:
    // (z.B. in CorePluginPackage: public string Id => "recopa.core";)
    private const string CorePluginId = "recopa.core";

    public ObservableCollection<PluginItemViewModel> Plugins { get; } = new();

    public ICommand RefreshPluginsCommand { get; }
    public ICommand AddPluginCommand { get; }

    private readonly PluginStateStore _state;

    // Falls dein Plugin-UI irgendein Grid/Layout braucht:
    // (damit der PluginItemViewModel-Konstruktor satisfied ist)
    public int GridColumns { get; set; } = 2;
    public int GridRows { get; set; } = 2;

    public PluginManagerViewModel()
    {
        var pluginDir = GetPluginDirectory();
        _state = new PluginStateStore(pluginDir);

        RefreshPluginsCommand = new RelayCommand(LoadPlugins);
        AddPluginCommand = new AsyncRelayCommand(AddPluginAsync);

        LoadPlugins();
    }

    public bool IsCorePluginId(string id)
        => string.Equals(id, CorePluginId, StringComparison.OrdinalIgnoreCase);

    public void SetEnabled(PluginItemViewModel plugin, bool enabled)
    {
        if (plugin.IsCorePlugin) enabled = false; // enforce
        _state.SetEnabled(plugin.Id, enabled);

        // Optional: falls Enabled/Disabled Einfluss auf das Laden haben soll:
        // LoadPlugins();
    }

    public void RemovePlugin(PluginItemViewModel plugin)
    {
        if (plugin.IsCorePlugin) return;

        try
        {
            var path = plugin.FilePath;
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // ignore
        }

        LoadPlugins();
    }

    private void LoadPlugins()
    {
        Plugins.Clear();

        // Wichtig: sicherstellen, dass PluginManager den Path kennt.
        // Wenn du SetPath woanders machst, ist ok. Sonst:
        // App.PluginManager!.SetPath(GetPluginDirectory());

        App.PluginManager!.Load();

        foreach (var plugin in App.PluginManager.Plugins)
        {
            var enabledDefault = true;

            // core: disabled by default
            if (IsCorePluginId(plugin.Id))
                enabledDefault = false;

            var enabled = _state.GetEnabled(plugin.Id, enabledDefault);

            // ✅ FIX: fehlende Parameter gridColumns / gridRows mitgeben
            Plugins.Add(new PluginItemViewModel(
                this,
                plugin,
                enabled,
                gridColumns: GridColumns,
                gridRows: GridRows));
        }
    }

    private async Task AddPluginAsync()
    {
        var lifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var window = lifetime?.MainWindow;
        if (window is null) return;

        var storage = window.StorageProvider;

        var options = new FilePickerOpenOptions
        {
            Title = "Select Plugin DLL",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Plugin DLL")
                {
                    Patterns = new[] { "*.dll" }
                }
            }
        };

        var result = await storage.OpenFilePickerAsync(options);
        var file = result.FirstOrDefault();
        if (file is null) return;

        var targetDir = GetPluginDirectory();
        Directory.CreateDirectory(targetDir);

        var targetPath = Path.Combine(targetDir, Path.GetFileName(file.Path.LocalPath));

        await using var source = await file.OpenReadAsync();
        await using var dest = File.Create(targetPath);
        await source.CopyToAsync(dest);

        LoadPlugins();
    }

    public static string GetPluginDirectory()
    {
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        if (OperatingSystem.IsMacOS())
        {
            basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                "Library", "Application Support");
        }

        if (OperatingSystem.IsLinux())
        {
            basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                ".local", "share");
        }

        return Path.Combine(basePath, "ReCoPa", "Plugins");
    }
}