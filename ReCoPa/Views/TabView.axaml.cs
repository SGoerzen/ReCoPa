using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ReCoPa.Views;

public partial class TabView : UserControl
{
    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<TabView, string?>(nameof(Header));

    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<TabView, bool>(nameof(IsActive));

    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public TabView()
    {
        InitializeComponent();
        // WICHTIG: KEIN DataContext = new TabViewModel();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}