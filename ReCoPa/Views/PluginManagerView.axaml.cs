using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using ReCoPa.ViewModels;

namespace ReCoPa.Views;

public partial class PluginManagerView : UserControl
{
    public PluginManagerView()
    {
        InitializeComponent();
    }

    private async void OnContributorClicked(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not TextBlock tb) return;

        // Dein struct Contributor hat Felder, keine Properties:
        if (tb.DataContext is ReCoPa.Plugins.Contributor c && !string.IsNullOrWhiteSpace(c.Github))
        {
            var top = TopLevel.GetTopLevel(this);
            if (top?.StorageProvider is null) return;

            await top.Launcher.LaunchUriAsync(new Uri(c.Github));
        }
    }

    private void Button_OpenFolder(object? sender, RoutedEventArgs e)
    {
        App.PluginManager.OpenFolderExplorer();
    }
}