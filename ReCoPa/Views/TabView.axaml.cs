using System.Windows.Input;
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

    // null = unknown/connecting
    public static readonly StyledProperty<bool?> IsConnectedProperty =
        AvaloniaProperty.Register<TabView, bool?>(nameof(IsConnected));

    public static readonly StyledProperty<ICommand?> SelectCommandProperty =
        AvaloniaProperty.Register<TabView, ICommand?>(nameof(SelectCommand));

    public static readonly StyledProperty<object?> SelectCommandParameterProperty =
        AvaloniaProperty.Register<TabView, object?>(nameof(SelectCommandParameter));

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

    public bool? IsConnected
    {
        get => GetValue(IsConnectedProperty);
        set => SetValue(IsConnectedProperty, value);
    }

    public ICommand? SelectCommand
    {
        get => GetValue(SelectCommandProperty);
        set => SetValue(SelectCommandProperty, value);
    }

    public object? SelectCommandParameter
    {
        get => GetValue(SelectCommandParameterProperty);
        set => SetValue(SelectCommandParameterProperty, value);
    }

    // Used by XAML binding
    public string StatusDotColor =>
        IsConnected switch
        {
            true => "#22C55E",   // green
            false => "#EF4444",  // red
            _ => "#94A3B8"       // gray
        };

    public TabView()
    {
        InitializeComponent();

        // ensure StatusDotColor refreshes when IsConnected changes
        this.GetObservable(IsConnectedProperty)
            .Subscribe(System.Reactive.Observer.Create<bool?>(_ =>
                InvalidateVisual()));
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}