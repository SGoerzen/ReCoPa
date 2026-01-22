using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using ReCoPa.Models;
using ReCoPa.Network;

namespace ReCoPa.ViewModels;

public class MainViewModel : ReactiveObject
{
    private readonly SocketServerHost _server;

    public ObservableCollection<XRClient> Clients { get; }

    public ReactiveCommand<Unit, Unit> QuitAppCommand { get; }
    public ReactiveCommand<Unit, Unit> QuitAllClientsCommand { get; }

    private bool _hasClients;
    public bool HasClients
    {
        get => _hasClients;
        private set => this.RaiseAndSetIfChanged(ref _hasClients, value);
    }

    public MainViewModel(SocketServerHost server)
    {
        _server = server;
        Clients = new ObservableCollection<XRClient>();

        // --- keep HasClients in sync (and make sure the observable is UI-scheduled)
        this.WhenAnyValue(x => x.Clients.Count)
            .Select(count => count > 0)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(v => HasClients = v);

        // --- update list on connect/disconnect
        _server.ClientConnected += conn =>
        {
            var model = new XRClient(conn);

            // avoid duplicates (in case of reconnects)
            if (!Clients.Any(x => x.Id == model.Id))
                Clients.Add(model);
        };

        _server.ClientDisconnected += conn =>
        {
            var item = Clients.FirstOrDefault(x => x.Id == conn.Id);
            if (item != null)
                Clients.Remove(item);
        };

        // --- Command: quit app
        QuitAppCommand = ReactiveCommand.Create(() =>
        {
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