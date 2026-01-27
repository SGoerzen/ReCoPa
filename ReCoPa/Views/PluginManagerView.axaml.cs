using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ReCoPa.ViewModels;

namespace ReCoPa.Views;

public partial class PluginManagerView : UserControl
{
    public PluginManagerView()
    {
        InitializeComponent();

        // Ensure bindings work (otherwise Commands are null -> buttons disabled)
        if (DataContext is null)
            DataContext = new PluginManagerViewModel();
    }

    private async void OnContributorClicked(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not TextBlock tb) return;

        // Contributor is a struct with fields -> pattern match works
        if (tb.DataContext is ReCoPa.Plugins.Contributor c &&
            !string.IsNullOrWhiteSpace(c.Github) &&
            Uri.TryCreate(c.Github, UriKind.Absolute, out var uri))
        {
            var top = TopLevel.GetTopLevel(this);
            if (top?.Launcher is null) return;

            await top.Launcher.LaunchUriAsync(uri);
        }
    }

    private void Button_OpenFolder(object? sender, RoutedEventArgs e)
    {
        // Keep logic centralized if you later move to a VM command.
        App.PluginManager?.OpenFolderExplorer();
    }

    private void Button_InstallPlugin(object? sender, RoutedEventArgs e)
    {
        // Execute the VM command (no NotImplementedException)
        if (DataContext is PluginManagerViewModel vm &&
            vm.AddPluginCommand?.CanExecute(null) == true)
        {
            vm.AddPluginCommand.Execute(null);
        }
    }
}