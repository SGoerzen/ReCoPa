using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using ReCoPa.Models;
using ReCoPa.Network;
using SukiUI.Toasts;

namespace ReCoPa.ViewModels;

public class MainViewModel : ViewModelBase
{

    private readonly SocketServerHost _server;

    public ObservableCollection<XRClient> Clients { get; }
    public ClientsTableViewModel ClientsTableViewModel { get; }

    public ReactiveCommand<Unit, Unit> QuitAppCommand { get; }
    public ReactiveCommand<Unit, Unit> QuitAllClientsCommand { get; }

    public ViewModelBase CurrentViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    public DashboardViewModel Dashboard { get; }
    public SettingsViewModel Settings { get; }

    public ICommand NavigateDashboardCommand { get; }
    public ICommand NavigateSettingsCommand { get; }

    public bool HasClients { get; private set; }

    public MainViewModel(SocketServerHost server)
    {
        _server = server;
        Clients = new ObservableCollection<XRClient>();
        ClientsTableViewModel = new ClientsTableViewModel(Clients);
        
        Dashboard = new DashboardViewModel(_server);
        Settings = new SettingsViewModel();
        
        CurrentViewModel = Dashboard;

        NavigateDashboardCommand = new RelayCommand(() =>
            CurrentViewModel = Dashboard);

        NavigateSettingsCommand = new RelayCommand(() =>
            CurrentViewModel = Settings);

        // --- keep HasClients in sync (and make sure the observable is UI-scheduled)
        this.WhenAnyValue(x => x.Clients.Count)
            .Select(count => count > 0)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(v => HasClients = v);

        // --- update list on connect/disconnect
        _server.ClientConnected += conn =>
        {
            Console.WriteLine($"Client connected: {conn.RemoteEndPoint}");
            var model = new XRClient(conn);
            if (!Clients.Any(x => x.Id == model.Id))
            {
                // 🔥 UI-Thread verwenden!
                Dispatcher.UIThread.Post(() =>
                {
                    Clients.Add(model);
                    HasClients = Clients.Count > 0;
                });
            }
        };

        _server.ClientDisconnected += conn =>
        {
            Console.WriteLine($"Client disconnected: {conn.RemoteEndPoint}");
            var item = Clients.FirstOrDefault(x => x.Id == conn.Id);
            if (item != null)
            {
                // 🔥 UI-Thread verwenden!
                Dispatcher.UIThread.Post(() =>
                {
                    Clients.Remove(item);
                    HasClients = Clients.Count > 0;
                });
            }
        };


        // --- Command: quit app
        QuitAppCommand = ReactiveCommand.Create(() =>
        {
            Console.WriteLine("Quitting app...");
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.Shutdown();
        },
        outputScheduler: RxApp.MainThreadScheduler);

        // --- Command: broadcast quit to all clients
        // CanExecute depends on HasClients; ensure it’s observed on UI thread
        var canQuitAll = this.WhenAnyValue(x => x.HasClients)
                             .ObserveOn(RxApp.MainThreadScheduler);

        QuitAllClientsCommand = ReactiveCommand.CreateFromTask(
            async () => await _server.BroadcastAsync("clients:quit"),
            canQuitAll,
            outputScheduler: RxApp.MainThreadScheduler
        );
    }
}
