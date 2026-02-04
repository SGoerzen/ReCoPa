using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ReCoPa.Views;

public partial class SidebarView : UserControl
{
    public SidebarView()
    {
        InitializeComponent();
        ScoreProgressValue = 51.0;
    }

    private void InitializeComponent()
        => AvaloniaXamlLoader.Load(this);
    
    public static readonly StyledProperty<bool> IsDisconnectedProperty =
        AvaloniaProperty.Register<SidebarView, bool>(nameof(IsDisconnected));

    public bool IsDisconnected
    {
        get => GetValue(IsDisconnectedProperty);
        set => SetValue(IsDisconnectedProperty, value);
    }

    public static readonly StyledProperty<string?> ClientNameProperty =
        AvaloniaProperty.Register<SidebarView, string?>(nameof(ClientName));

    public static readonly StyledProperty<bool> IsConnectedProperty =
        AvaloniaProperty.Register<SidebarView, bool>(nameof(IsConnected));

    public static readonly StyledProperty<int> StatementsCountProperty =
        AvaloniaProperty.Register<SidebarView, int>(nameof(StatementsCount));

    public static readonly StyledProperty<int> GameObjectsCountProperty =
        AvaloniaProperty.Register<SidebarView, int>(nameof(GameObjectsCount));
    
    public static readonly StyledProperty<double> ScoreProgressValueProperty =
        AvaloniaProperty.Register<SidebarView, double>(nameof(ScoreProgressValue));

    public static readonly StyledProperty<double> FpsProperty =
        AvaloniaProperty.Register<SidebarView, double>(nameof(Fps));

    public static readonly StyledProperty<int> HeartRateProperty =
        AvaloniaProperty.Register<SidebarView, int>(nameof(HeartRate));

    public static readonly StyledProperty<bool> IsSessionSelectedProperty =
        AvaloniaProperty.Register<SidebarView, bool>(nameof(IsSessionSelected));
    
    public static readonly StyledProperty<TimeSpan> ElapsedTimeProperty =
        AvaloniaProperty.Register<SidebarView, TimeSpan>(nameof(ElapsedTime));

    public static readonly StyledProperty<ICommand?> NavigateVisualizationsCommandProperty =
        AvaloniaProperty.Register<SidebarView, ICommand?>(nameof(NavigateVisualizationsCommand));

    public static readonly StyledProperty<ICommand?> NavigateSettingsCommandProperty =
        AvaloniaProperty.Register<SidebarView, ICommand?>(nameof(NavigateSettingsCommand));

    public static readonly StyledProperty<ICommand?> StartEyeCalibrationCommandProperty =
        AvaloniaProperty.Register<SidebarView, ICommand?>(nameof(StartEyeCalibrationCommand));

    public static readonly StyledProperty<ICommand?> PauseTrackingCommandProperty =
        AvaloniaProperty.Register<SidebarView, ICommand?>(nameof(PauseTrackingCommand));

    public static readonly StyledProperty<ICommand?> StopTrackingCommandProperty =
        AvaloniaProperty.Register<SidebarView, ICommand?>(nameof(StopTrackingCommand));

    public static readonly StyledProperty<ICommand?> ShutdownUnityAppCommandProperty =
        AvaloniaProperty.Register<SidebarView, ICommand?>(nameof(ShutdownUnityAppCommand));

    public string? ClientName { get => GetValue(ClientNameProperty); set => SetValue(ClientNameProperty, value); }
    public bool IsConnected { get => GetValue(IsConnectedProperty); set => SetValue(IsConnectedProperty, value); }
    public int StatementsCount { get => GetValue(StatementsCountProperty); set => SetValue(StatementsCountProperty, value); }
    public int GameObjectsCount { get => GetValue(GameObjectsCountProperty); set => SetValue(GameObjectsCountProperty, value); }
    public double ScoreProgressValue { get => GetValue(ScoreProgressValueProperty); set => SetValue(ScoreProgressValueProperty, value); }
    public double Fps { get => GetValue(FpsProperty); set => SetValue(FpsProperty, value); }
    public int HeartRate { get => GetValue(HeartRateProperty); set => SetValue(HeartRateProperty, value); }
    public bool IsSessionSelected { get => GetValue(IsSessionSelectedProperty); set => SetValue(IsSessionSelectedProperty, value); }
    public TimeSpan ElapsedTime { get => GetValue(ElapsedTimeProperty); set => SetValue(ElapsedTimeProperty, value); }
    
    public ICommand? NavigateVisualizationsCommand { get => GetValue(NavigateVisualizationsCommandProperty); set => SetValue(NavigateVisualizationsCommandProperty, value); }
    public ICommand? NavigateSettingsCommand { get => GetValue(NavigateSettingsCommandProperty); set => SetValue(NavigateSettingsCommandProperty, value); }

    public ICommand? StartEyeCalibrationCommand { get => GetValue(StartEyeCalibrationCommandProperty); set => SetValue(StartEyeCalibrationCommandProperty, value); }
    public ICommand? PauseTrackingCommand { get => GetValue(PauseTrackingCommandProperty); set => SetValue(PauseTrackingCommandProperty, value); }
    public ICommand? StopTrackingCommand { get => GetValue(StopTrackingCommandProperty); set => SetValue(StopTrackingCommandProperty, value); }
    public ICommand? ShutdownUnityAppCommand { get => GetValue(ShutdownUnityAppCommandProperty); set => SetValue(ShutdownUnityAppCommandProperty, value); }
}
