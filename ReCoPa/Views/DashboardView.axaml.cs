using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using ReCoPa.Extensions;
using ReCoPa.Services;
using SukiUI.Toasts;
namespace ReCoPa.Views;

public partial class DashboardView : UserControl
{

    public DashboardView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void OpenRecopaPackageLink(object? sender, RoutedEventArgs e)
    {
        const string url = "https://www.npmjs.com/package/com.rwth.unity.omilaxr.recopa";
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    private async void InstallUnityPackage(object? sender, RoutedEventArgs e)
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

        var status = await UnityManifestUpdater.UpdateAsync(path);
        MainWindow.ToastManager.CreateToast()
            .WithTitle("Unity manifest")
            .WithContent(status)
            .QuickShow();
    }
}
