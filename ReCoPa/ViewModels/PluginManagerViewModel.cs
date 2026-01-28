using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReCoPa.Plugins;
using ReCoPa.Views;
using SukiUI.Toasts;

namespace ReCoPa.ViewModels;

public sealed partial class PluginManagerViewModel : ViewModelBase
{
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
        var pluginDir = PluginManager.GetPluginDirectory();
        _state = new PluginStateStore(pluginDir);

        RefreshPluginsCommand = new RelayCommand(LoadPlugins);
        AddPluginCommand = new AsyncRelayCommand(AddPluginAsync);

        LoadPlugins();
    }

    public void SetEnabled(PluginItemViewModel plugin, bool enabled)
    {
        _state.SetEnabled(plugin.Id, enabled);

        // Optional: falls Enabled/Disabled Einfluss auf das Laden haben soll:
        // LoadPlugins();
    }

    public void RemovePlugin(PluginItemViewModel plugin)
    {
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
        try
        {
            Plugins.Clear();

            App.PluginManager!.Load();

            foreach (var plugin in App.PluginManager.Plugins)
            {
                var enabled = _state.GetEnabled(plugin.Id, true);

                Plugins.Add(new PluginItemViewModel(
                    this,
                    plugin,
                    enabled,
                    gridColumns: GridColumns,
                    gridRows: GridRows));
            }
        }
        catch (Exception ex)
        {
            var toast = MainWindow.ToastManager.CreateToast()
                .OfType(NotificationType.Error)
                .WithTitle("Cannot load plugins.")
                .WithContent(ex.Message);
            
            toast.SetCanDismissByClicking(true);
            toast.Queue();
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

        var targetDir = PluginManager.GetPluginDirectory();
        Directory.CreateDirectory(targetDir);

        var targetPath = Path.Combine(
            targetDir,
            Path.GetFileName(file.Name) // <- use Name, not LocalPath
        );

        Console.WriteLine($"Copying '{file.Name}' -> '{targetPath}'");

        try
        {
            await using var src = await file.OpenReadAsync();
            await using var dst = File.Create(targetPath);
            await src.CopyToAsync(dst);
            await dst.FlushAsync();

            Console.WriteLine("Copy OK.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Copy FAILED: {ex}");
        }
        
        LoadPlugins();
        
        var toast = MainWindow.ToastManager.CreateToast()
            .OfType(NotificationType.Success)
            .WithTitle("Install Plugin")
            .WithContent($"Installed plugin {Path.GetFileName(targetPath)}, successfully.");
        toast.SetCanDismissByClicking(true);
        toast.Queue();
    }
}