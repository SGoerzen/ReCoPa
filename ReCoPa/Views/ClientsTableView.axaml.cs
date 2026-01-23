using Avalonia.Controls;
using ReCoPa.ViewModels;

namespace ReCoPa.Views;

public partial class ClientsTableView : UserControl
{
    public ClientsTableView()
    {
        InitializeComponent();
    }

    public ClientsTableView(ClientsTableViewModel vm)
    {
        InitializeComponent();
        DataContext = vm; 
    }
}