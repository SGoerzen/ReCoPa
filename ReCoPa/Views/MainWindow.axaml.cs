using System.Reflection;
using Avalonia.Controls;
using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace ReCoPa.Views;

public partial class MainWindow : SukiWindow
{
    public static ISukiToastManager ToastManager = new SukiToastManager();
    private static Window? _pluginWindow;
    public static ISukiDialogManager DialogManager = new SukiDialogManager();

    public MainWindow()
    {
        InitializeComponent();
        ToastHost.Manager = ToastManager;
        DialogHost.Manager = DialogManager;
        Title = Title + " v" + (Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown");
    }
    
    private void OpenPlugins(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_pluginWindow != null)
        {
            _pluginWindow.Activate();
            return;
        }

        _pluginWindow = new Window
        {
            Title = "Plugins",
            MinWidth = 800,
            Width = 800,
            MinHeight = 400,
            Height = 400,
            Content = new PluginManagerView()
        };

        _pluginWindow.Closed += (_, __) => _pluginWindow = null;
        _pluginWindow.Show();
    }
}
