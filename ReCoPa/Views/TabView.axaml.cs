using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReCoPa.ViewModels;

namespace ReCoPa.Views;

public partial class TabView : UserControl
{
    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<TabView, string?>(nameof(Header));

    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<TabView, bool>(nameof(IsActive));

    public static readonly StyledProperty<TabConnectionState> ConnectionStateProperty =
        AvaloniaProperty.Register<TabView, TabConnectionState>(nameof(ConnectionState));

    public static readonly StyledProperty<ICommand?> SelectCommandProperty =
        AvaloniaProperty.Register<TabView, ICommand?>(nameof(SelectCommand));

    public static readonly StyledProperty<object?> SelectCommandParameterProperty =
        AvaloniaProperty.Register<TabView, object?>(nameof(SelectCommandParameter));

    public static readonly StyledProperty<ICommand?> CloseCommandProperty =
        AvaloniaProperty.Register<TabView, ICommand?>(nameof(CloseCommand));

    public static readonly StyledProperty<object?> CloseCommandParameterProperty =
        AvaloniaProperty.Register<TabView, object?>(nameof(CloseCommandParameter));

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

    public TabConnectionState ConnectionState
    {
        get => GetValue(ConnectionStateProperty);
        set => SetValue(ConnectionStateProperty, value);
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

    public ICommand? CloseCommand
    {
        get => GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    public object? CloseCommandParameter
    {
        get => GetValue(CloseCommandParameterProperty);
        set => SetValue(CloseCommandParameterProperty, value);
    }

    public TabView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
