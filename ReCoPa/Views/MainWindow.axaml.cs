using System.Reflection;
using Avalonia.Controls;
using SukiUI.Controls;
using SukiUI.Toasts;

namespace ReCoPa.Views;

public partial class MainWindow : SukiWindow
{
    public static ISukiToastManager ToastManager = new SukiToastManager();

    public MainWindow()
    {
        InitializeComponent();
        ToastHost.Manager = ToastManager;
        Title = Title + " v" + (Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown");
    }
    
    private void OpenPlugins(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var window = new Window
        {
            Title = "Plugins",
            MinWidth = 800,
            Width = 800,
            MinHeight = 400,
            Height = 400,
            Content = new PluginManagerView()
        };

        window.Show();
    }
}