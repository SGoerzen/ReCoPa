using System.Diagnostics;
using ReCoPa.Models;
using ReCoPa.Network;

namespace ReCoPa;

public partial class App : Application
{
    private readonly SocketServerHost _server;
    private bool _started;

    public App(SocketServerHost server)
    {
        InitializeComponent();
        _server = server;

        // These should be raised by your SocketServerService when a TCP client connects/disconnects
        _server.ClientConnected += connection =>
        {
            Debug.WriteLine($"[ReCoPa] Client connected: {connection.RemoteEndPoint}");
            Console.WriteLine($"[ReCoPa] Client connected: {connection.RemoteEndPoint}");
        };

        _server.ClientDisconnected += connection =>
        {
            Debug.WriteLine($"[ReCoPa] Client disconnected: {connection.RemoteEndPoint}");
            Console.WriteLine($"[ReCoPa] Client disconnected: {connection.RemoteEndPoint}");
        };
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Start here so exceptions/logs happen in the MAUI lifecycle and you don't start twice.
        if (!_started)
        {
            _started = true;
            _ = StartServerSafeAsync();
            _server.On<TrackingMeta>("clients:meta", tm =>
            {
                Console.WriteLine($"[ReCoPa] Meta received: {tm}");
            });
        }

        var window = new Window(new AppShell());
        window.Title = "ReCoPa V2";
        window.Width = 1440;
        window.Height = 900;

        window.Destroying += (_, __) =>
        {
            // fire-and-forget, aber sauber
            _server.DisposeAsync().AsTask();
        };
        
        AppDomain.CurrentDomain.ProcessExit += (_, __) =>
        {
            _server.DisposeAsync().AsTask();
        };
        
        return window;
    }

    private async Task StartServerSafeAsync()
    {
        try
        {
            Debug.WriteLine("[ReCoPa] Starting SocketServer on port 4567...");
            Console.WriteLine("[ReCoPa] Starting SocketServer on port 4567...");

            await _server.StartAsync(4567);

            Debug.WriteLine("[ReCoPa] SocketServer STARTED on port 4567.");
            Console.WriteLine("[ReCoPa] SocketServer STARTED on port 4567.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[ReCoPa] SocketServer FAILED to start: " + ex);
            Console.WriteLine("[ReCoPa] SocketServer FAILED to start: " + ex);
        }
    }
}