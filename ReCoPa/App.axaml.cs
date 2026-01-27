using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using ReactiveUI;
using ReCoPa.Network;
using ReCoPa.Plugins;
using ReCoPa.ViewModels;
using ReCoPa.Views;
using SukiUI;
using SukiUI.Models;
using Avalonia.Media;

namespace ReCoPa;

public partial class App : Application
{
    public static PluginManager PluginManager { get; } = new();
    public static PluginStateStore PluginStateStore { get; private set; }
    
    public static SocketServerHost? Socket { get; private set; }
    private int _disposed; 
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        LoadPlugins();
    }

    public void LoadPlugins()
    {
        var pluginDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ReCoPa",
            "Plugins");
        
        PluginStateStore = new PluginStateStore(pluginDir);
        
        Directory.CreateDirectory(pluginDir);
        PluginManager.Load(pluginDir);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // ✅ ReactiveUI main-thread scheduler for Avalonia
        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;

        // ✅ Create server with Avalonia UI dispatcher
        Socket = new SocketServerHost(
            options: null,
            uiPost: a => Dispatcher.UIThread.Post(a)
        );

        // start server (fire-and-forget)
        _ = Socket.StartAsync(4567);

        // ✅ cover unexpected exits/crashes too
        HookShutdownHandlers();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            var window = new MainWindow
            {
                DataContext = new MainViewModel(Socket)
            };

            // User closes the window (X)
            window.Closing += async (_, __) => await DisposeServerOnceAsync();

            desktop.MainWindow = window;
            desktop.MainWindow.AttachDevTools(new KeyGesture(Key.F12));

            // Normal app exit
            desktop.Exit += async (_, __) => await DisposeServerOnceAsync();
        }
        
        InitTheme();

        base.OnFrameworkInitializationCompleted();
    }

    private void InitTheme()
    {
        var theme = SukiTheme.GetInstance();

        theme.ChangeBaseTheme(ThemeVariant.Light);

        // Logo-Farben
        var primary = Color.FromRgb(0, 80, 152);   // #005098
        var secondary = Color.FromRgb(0, 55, 105); // #003769

        var recoPaTheme = new SukiColorTheme("ReCoPa", primary, secondary);
        theme.AddColorTheme(recoPaTheme);
        theme.ChangeColorTheme(recoPaTheme);

    }

    private void HookShutdownHandlers()
    {
        AppDomain.CurrentDomain.ProcessExit += (_, __) =>
        {
            // best-effort, can't reliably await here
            DisposeServerOnceSync();
        };

        AppDomain.CurrentDomain.UnhandledException += (_, __) =>
        {
            DisposeServerOnceSync();
        };

        TaskScheduler.UnobservedTaskException += (_, __) =>
        {
            DisposeServerOnceSync();
        };
    }

    private async Task DisposeServerOnceAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1) return;

        var s = Socket;
        Socket = null;
        if (s == null) return;

        try
        {
            // optional: timeout so shutdown never hangs
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
            await s.DisposeAsync().AsTask().WaitAsync(cts.Token);
        }
        catch
        {
            // swallow during shutdown
        }
    }

    private void DisposeServerOnceSync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1) return;

        var s = Socket;
        Socket = null;
        if (s == null) return;

        try
        {
            // best-effort sync wait (process is exiting)
            s.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
        catch
        {
            // swallow during shutdown/crash
        }
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var plugins =
            BindingPlugins.DataValidators
                .OfType<DataAnnotationsValidationPlugin>()
                .ToArray();

        foreach (var plugin in plugins)
            BindingPlugins.DataValidators.Remove(plugin);
    }
}