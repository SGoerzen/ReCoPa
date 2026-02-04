using System;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ReCoPa.ViewModels;

public partial class SessionViewModel : ViewModelBase, IDisposable
{
    private readonly DateTime _startedAtUtc = DateTime.UtcNow;
    private readonly DispatcherTimer _timer;

    public VisualizationContainerViewModel Visualization { get; } = new();
    public SessionSettingsViewModel Settings { get; } = new();

    [ObservableProperty] private string? clientName;
    [ObservableProperty] private bool isConnected;
    [ObservableProperty] private int statementsCount;
    [ObservableProperty] private int gameObjectsCount;
    [ObservableProperty] private double fps;
    [ObservableProperty] private int heartRate;
    [ObservableProperty] private TimeSpan elapsedTime = TimeSpan.Zero;
    [ObservableProperty] private bool isSessionSelected = true;
    [ObservableProperty] private bool isEyeTrackingEnabled = true;
    [ObservableProperty] private string currentView = "Visualizations";

    public bool IsVisualizationsView => CurrentView == "Visualizations";
    public bool IsSettingsView => CurrentView == "Settings";

    public bool IsDisconnected => !IsConnected;

    public SessionViewModel(string? clientName = null)
    {
        ClientName = clientName ?? "Session";

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += OnTick;
        _timer.Start();
    }

    [RelayCommand]
    private void NavigateVisualizations()
    {
        CurrentView = "Visualizations";
    }

    [RelayCommand]
    private void NavigateSettings()
    {
        CurrentView = "Settings";
    }

    [RelayCommand]
    private void StartCalibration()
    {
        // TODO: call calibration logic per session
        Fps = Math.Max(0, Fps - 0.5);
    }

    [RelayCommand]
    private void PauseTracking()
    {
        // TODO
    }

    [RelayCommand]
    private void StopTracking()
    {
        // TODO
    }

    [RelayCommand]
    private void ShutdownApp()
    {
        // TODO: send shutdown to Unity client
        IsConnected = false;
    }

    partial void OnIsConnectedChanged(bool value)
    {
        OnPropertyChanged(nameof(IsDisconnected));
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= OnTick;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        ElapsedTime = DateTime.UtcNow - _startedAtUtc;
    }

    partial void OnCurrentViewChanged(string value)
    {
        OnPropertyChanged(nameof(IsVisualizationsView));
        OnPropertyChanged(nameof(IsSettingsView));
    }
}
