// Views/SettingsView.axaml.cs
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using ReCoPa.ViewModels;

namespace ReCoPa.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private async void SelectUnityManifest(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider == null)
            return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Unity Packages/manifest.json",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Unity manifest")
                {
                    Patterns = ["manifest.json"],
                    MimeTypes = ["application/json"]
                }
            ]
        });

        if (files.Count == 0)
            return;

        var path = files[0].TryGetLocalPath();
        if (string.IsNullOrWhiteSpace(path))
            return;

        if (DataContext is SettingsViewModel vm)
            await vm.UpdateUnityManifestAsync(path);
    }
}
