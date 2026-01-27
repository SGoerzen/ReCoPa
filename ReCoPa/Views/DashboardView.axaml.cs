using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReCoPa.ViewModels;

namespace ReCoPa.Views;

public partial class DashboardView : UserControl
{

    public DashboardView()
    {
        InitializeComponent();
        DataContext = new DashboardViewModel();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}