using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Material.Icons;

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
    
    public static readonly StyledProperty<bool> IsAwaitingConnectionProperty =
        AvaloniaProperty.Register<SidebarView, bool>(nameof(IsAwaitingConnection));

    public bool IsDisconnected
    {
        get => GetValue(IsDisconnectedProperty);
        set => SetValue(IsDisconnectedProperty, value);
    }
    
    public bool IsAwaitingConnection
    {
        get => GetValue(IsAwaitingConnectionProperty);
        set => SetValue(IsAwaitingConnectionProperty, value);
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

    public static readonly StyledProperty<ICommand?> StartStopTrackingCommandProperty =
        AvaloniaProperty.Register<SidebarView, ICommand?>(nameof(StartStopTrackingCommand));

    public static readonly StyledProperty<ICommand?> ShutdownUnityAppCommandProperty =
        AvaloniaProperty.Register<SidebarView, ICommand?>(nameof(ShutdownUnityAppCommand));

    public static readonly StyledProperty<ICommand?> StartEditSessionNameCommandProperty =
        AvaloniaProperty.Register<SidebarView, ICommand?>(nameof(StartEditSessionNameCommand));

    public static readonly StyledProperty<ICommand?> SaveSessionNameCommandProperty =
        AvaloniaProperty.Register<SidebarView, ICommand?>(nameof(SaveSessionNameCommand));

    public static readonly StyledProperty<ICommand?> CancelSessionNameEditCommandProperty =
        AvaloniaProperty.Register<SidebarView, ICommand?>(nameof(CancelSessionNameEditCommand));

    public static readonly StyledProperty<bool> IsVisualizationsViewProperty =
        AvaloniaProperty.Register<SidebarView, bool>(nameof(IsVisualizationsView));

    public static readonly StyledProperty<bool> IsSettingsViewProperty =
        AvaloniaProperty.Register<SidebarView, bool>(nameof(IsSettingsView));

    public static readonly StyledProperty<bool> IsTrackingRunningProperty =
        AvaloniaProperty.Register<SidebarView, bool>(nameof(IsTrackingRunning));

    public static readonly StyledProperty<string> StartStopTextProperty =
        AvaloniaProperty.Register<SidebarView, string>(nameof(StartStopText), "Stop");

    public static readonly StyledProperty<MaterialIconKind> StartStopIconProperty =
        AvaloniaProperty.Register<SidebarView, MaterialIconKind>(nameof(StartStopIcon), MaterialIconKind.Stop);

    public static readonly StyledProperty<bool> IsEditingSessionNameProperty =
        AvaloniaProperty.Register<SidebarView, bool>(nameof(IsEditingSessionName));

    public static readonly StyledProperty<string> SessionNameEditProperty =
        AvaloniaProperty.Register<SidebarView, string>(nameof(SessionNameEdit), string.Empty);

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
    public ICommand? StartStopTrackingCommand { get => GetValue(StartStopTrackingCommandProperty); set => SetValue(StartStopTrackingCommandProperty, value); }
    public ICommand? ShutdownUnityAppCommand { get => GetValue(ShutdownUnityAppCommandProperty); set => SetValue(ShutdownUnityAppCommandProperty, value); }
    public ICommand? StartEditSessionNameCommand { get => GetValue(StartEditSessionNameCommandProperty); set => SetValue(StartEditSessionNameCommandProperty, value); }
    public ICommand? SaveSessionNameCommand { get => GetValue(SaveSessionNameCommandProperty); set => SetValue(SaveSessionNameCommandProperty, value); }
    public ICommand? CancelSessionNameEditCommand { get => GetValue(CancelSessionNameEditCommandProperty); set => SetValue(CancelSessionNameEditCommandProperty, value); }

    public bool IsVisualizationsView { get => GetValue(IsVisualizationsViewProperty); set => SetValue(IsVisualizationsViewProperty, value); }
    public bool IsSettingsView { get => GetValue(IsSettingsViewProperty); set => SetValue(IsSettingsViewProperty, value); }
    public bool IsTrackingRunning { get => GetValue(IsTrackingRunningProperty); set => SetValue(IsTrackingRunningProperty, value); }
    public string StartStopText { get => GetValue(StartStopTextProperty); set => SetValue(StartStopTextProperty, value); }
    public MaterialIconKind StartStopIcon { get => GetValue(StartStopIconProperty); set => SetValue(StartStopIconProperty, value); }
    public bool IsEditingSessionName { get => GetValue(IsEditingSessionNameProperty); set => SetValue(IsEditingSessionNameProperty, value); }
    public string SessionNameEdit { get => GetValue(SessionNameEditProperty); set => SetValue(SessionNameEditProperty, value); }
}
