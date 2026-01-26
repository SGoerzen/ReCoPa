using Avalonia.Controls;
using SukiUI.Controls;

namespace ReCoPa.Views;

public partial class MainWindow : SukiWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }
    
    private void OpenPlugins(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var window = new Window
        {
            Title = "Plugins",
            Width = 500,
            Height = 400,
            Content = new PluginManagerView()
        };

        window.Show();
    }
}