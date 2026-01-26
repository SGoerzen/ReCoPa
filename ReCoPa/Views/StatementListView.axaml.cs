using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReCoPa.ViewModels;

namespace ReCoPa.Views;

public partial class StatementListView : UserControl
{
    public StatementListView()
    {
        InitializeComponent();
        DataContext = new StatementListViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}