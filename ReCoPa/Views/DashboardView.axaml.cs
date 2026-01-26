using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReCoPa.ViewModels;

namespace ReCoPa.Views;

public partial class DashboardView : UserControl
{
    private ScrollViewer? _tabsScroll;

    public DashboardView()
    {
        InitializeComponent();
        DataContext = new DashboardViewModel();
        _tabsScroll = this.FindControl<ScrollViewer>("TabsScroll");
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void ScrollTabsLeft(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_tabsScroll is null) return;

        const double step = 220; // px pro Klick
        var x = _tabsScroll.Offset.X - step;
        if (x < 0) x = 0;

        _tabsScroll.Offset = new Vector(x, _tabsScroll.Offset.Y);
    }

    private void ScrollTabsRight(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_tabsScroll is null) return;

        const double step = 220; // px pro Klick
        var x = _tabsScroll.Offset.X + step;

        _tabsScroll.Offset = new Vector(x, _tabsScroll.Offset.Y);
    }
}