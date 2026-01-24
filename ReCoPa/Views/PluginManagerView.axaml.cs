using Avalonia.Controls;
using ReCoPa.ViewModels;

namespace ReCoPa.Views;

public partial class PluginManagerView : UserControl
{
    public PluginManagerView()
    {
        InitializeComponent();
        DataContext = new PluginManagerViewModel();
    }
}