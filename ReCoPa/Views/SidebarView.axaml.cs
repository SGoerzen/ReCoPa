using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ReCoPa.Views;

public partial class SidebarView : UserControl
{
    public static readonly StyledProperty<string?> ClientNameProperty =
        AvaloniaProperty.Register<SidebarView, string?>(nameof(ClientName));

    public string? ClientName
    {
        get => GetValue(ClientNameProperty);
        set => SetValue(ClientNameProperty, value);
    }

    public SidebarView()
    {
        InitializeComponent();
        // WICHTIG: KEIN DataContext = new SidebarViewModel();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}