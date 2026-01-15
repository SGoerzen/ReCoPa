using System.Collections.ObjectModel;
using ReCoPa.Models;

namespace ReCoPa.Views;

public partial class ClientsTableView : ContentView
{
    public ClientsTableView()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty ClientsProperty =
        BindableProperty.Create(
            nameof(Clients),
            typeof(ObservableCollection<XRClient>),
            typeof(ClientsTableView),
            default(ObservableCollection<XRClient>));

    public ObservableCollection<XRClient> Clients
    {
        get => (ObservableCollection<XRClient>)GetValue(ClientsProperty);
        set => SetValue(ClientsProperty, value);
    }

    private async void QuitButton_OnClicked(object? sender, EventArgs e)
    {
        if (sender is not Button b) return;
        if (b.CommandParameter is not XRClient client) return;

        // optimistic UI remove (optional)
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Clients?.Contains(client) == true)
                Clients.Remove(client);
        });

        try
        {
            await client.EmitAsync("clients:quit");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Quit failed: " + ex);
        }
    }
}