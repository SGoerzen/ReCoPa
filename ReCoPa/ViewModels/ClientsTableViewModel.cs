using System;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using ReCoPa.Models;

namespace ReCoPa.ViewModels;

public class ClientsTableViewModel : ReactiveObject
{
    private ObservableCollection<XRClient> _clients;

    public ObservableCollection<XRClient> Clients
    {
        get => _clients;
        set => this.RaiseAndSetIfChanged(ref _clients, value);
    }

    public ReactiveCommand<XRClient, Unit> QuitClientCommand { get; }

    public ClientsTableViewModel(ObservableCollection<XRClient> clients)
    {
        _clients = clients;

        QuitClientCommand = ReactiveCommand.CreateFromTask<XRClient>(async client =>
        {
            Clients.Remove(client);

            try
            {
                await client.EmitAsync("quit");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        });
    }
}