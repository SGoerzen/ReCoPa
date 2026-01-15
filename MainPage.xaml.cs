using System.Collections.ObjectModel;
using ReCoPa.Models;
using ReCoPa.Network;

namespace ReCoPa;

public partial class MainPage : ContentPage
{
    protected readonly SocketServerHost Socket;
    
    public ObservableCollection<XRClient> Clients { get; } = new();
    
    public MainPage(SocketServerHost socket)
    {
        InitializeComponent();

        Socket = socket;
        
        socket.ClientConnected += conn =>
        {
            var model = new XRClient(conn);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (!Clients.Any(x => x.Id == model.Id))
                    Clients.Add(model);
            });
        };

        socket.ClientDisconnected += conn =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var item = Clients.FirstOrDefault(x => x.Id == conn.Id);
                if (item != null)
                    Clients.Remove(item);
            });
        };
        
        socket.On<string>("clients:meta", payload =>
        {
            Console.WriteLine("[meta] " + payload);
        });
        
        BindingContext = this;

    }
    
    private async void KillAppBtn_OnClicked(object? sender, EventArgs e)
    {
        await Socket.BroadcastAsync("clients:quit");
    }
}