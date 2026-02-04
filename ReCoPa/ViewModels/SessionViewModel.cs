using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using ReCoPa;
using ReCoPa.Network;

namespace ReCoPa.ViewModels;

public partial class SessionViewModel : ViewModelBase, IDisposable
{
    private readonly DateTime _startedAtUtc = DateTime.UtcNow;
    private readonly DispatcherTimer _timer;
    private readonly SocketServerHost? _server;
    private readonly Guid? _clientId;
    private readonly List<IDisposable> _subscriptions = new();
    private readonly Random _rng = new();

    public VisualizationContainerViewModel Visualization { get; } = new();
    public SessionSettingsViewModel Settings { get; } = new();

    [ObservableProperty] private string? clientName;
    [ObservableProperty] private bool isConnected;
    [ObservableProperty] private int statementsCount;
    [ObservableProperty] private int gameObjectsCount;
    [ObservableProperty] private double fps;
    [ObservableProperty] private int heartRate;
    [ObservableProperty] private double scoreProgressValue;
    [ObservableProperty] private TimeSpan elapsedTime = TimeSpan.Zero;
    [ObservableProperty] private bool isSessionSelected = true;
    [ObservableProperty] private bool isEyeTrackingEnabled = true;
    [ObservableProperty] private bool isTrackingRunning = true;
    [ObservableProperty] private bool isTrackingPaused;
    [ObservableProperty] private string currentView = "Visualizations";

    public bool IsVisualizationsView => CurrentView == "Visualizations";
    public bool IsSettingsView => CurrentView == "Settings";

    public bool IsDisconnected => !IsConnected;

    public string StartStopText => IsTrackingRunning ? "Stop" : "Start";
    public MaterialIconKind StartStopIcon => IsTrackingRunning ? MaterialIconKind.Stop : MaterialIconKind.PlayCircle;

    public SessionViewModel(string? clientName = null, SocketServerHost? server = null, Guid? clientId = null)
    {
        ClientName = clientName ?? "Session";
        _server = server ?? App.Socket;
        _clientId = clientId;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += OnTick;
        _timer.Start();

        SubscribeToSocket();
        StartTracking();
    }

    [RelayCommand]
    private void NavigateVisualizations() => CurrentView = "Visualizations";

    [RelayCommand]
    private void NavigateSettings() => CurrentView = "Settings";

    [RelayCommand]
    private void StartCalibration()
    {
        _ = EmitAsync("clients:calibration:start");
    }

    [RelayCommand]
    private void PauseTracking()
    {
        if (!IsTrackingRunning)
            return;

        _ = EmitAsync("clients:tracking:pause");
        IsTrackingPaused = !IsTrackingPaused;
    }

    [RelayCommand]
    private void StopTracking()
    {
        _ = EmitAsync("clients:tracking:stop");
        IsTrackingRunning = false;
        IsTrackingPaused = false;
    }

    [RelayCommand]
    private void StartStopTracking()
    {
        if (IsTrackingRunning)
        {
            _ = EmitAsync("clients:tracking:stop");
            IsTrackingRunning = false;
            IsTrackingPaused = false;
            return;
        }

        StartTracking();
    }

    [RelayCommand]
    private void ShutdownApp()
    {
        _ = EmitAsync("clients:shutdown");
    }

    partial void OnIsConnectedChanged(bool value)
    {
        OnPropertyChanged(nameof(IsDisconnected));
    }

    partial void OnIsTrackingRunningChanged(bool value)
    {
        OnPropertyChanged(nameof(StartStopText));
        OnPropertyChanged(nameof(StartStopIcon));
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= OnTick;

        foreach (var sub in _subscriptions)
            sub.Dispose();
        _subscriptions.Clear();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        ElapsedTime = DateTime.UtcNow - _startedAtUtc;

        if (!IsConnected)
            SimulateMetrics();
    }

    partial void OnCurrentViewChanged(string value)
    {
        OnPropertyChanged(nameof(IsVisualizationsView));
        OnPropertyChanged(nameof(IsSettingsView));
    }

    private void SubscribeToSocket()
    {
        if (_server == null)
            return;

        _subscriptions.Add(_server.On("clients:meta", HandleMeta));
        _subscriptions.Add(_server.On("clients:statements", HandleStatement));
    }

    private void HandleMeta(string payload)
    {
        if (!TryReadPayload(payload, out var root))
            return;

        if (!IsForThisSession(root))
            return;

        if (TryReadInt(root, "statements", out var statements))
            StatementsCount = statements;
        if (TryReadInt(root, "gameObjects", out var gameObjects))
            GameObjectsCount = gameObjects;
        if (TryReadInt(root, "heartRate", out var heartRate))
            HeartRate = heartRate;
        if (TryReadDouble(root, "fps", out var fps))
            Fps = fps;
        if (TryReadDouble(root, "score", out var score))
            ScoreProgressValue = score;
    }

    private void HandleStatement(string payload)
    {
        if (!TryReadPayload(payload, out var root))
            return;

        if (!IsForThisSession(root))
            return;

        StatementsCount += 1;

        if (TryReadArrayLength(root, "gameObjects", out var count))
        {
            GameObjectsCount += count;
        }
        else if (TryReadString(root, "gameObject", out _)
                 || TryReadString(root, "gameObjectId", out _))
        {
            GameObjectsCount += 1;
        }
    }

    private bool IsForThisSession(JsonElement root)
    {
        if (_clientId == null)
            return true;

        if (TryReadString(root, "clientId", out var idText)
            || TryReadString(root, "client_id", out idText)
            || TryReadString(root, "id", out idText))
        {
            if (Guid.TryParse(idText, out var id))
                return id == _clientId;
        }

        if (root.TryGetProperty("client", out var clientEl)
            && TryReadString(clientEl, "id", out idText)
            && Guid.TryParse(idText, out var clientId))
        {
            return clientId == _clientId;
        }

        return true;
    }

    private static bool TryReadPayload(string payload, out JsonElement root)
    {
        root = default;
        if (string.IsNullOrWhiteSpace(payload))
            return false;

        try
        {
            using var doc = JsonDocument.Parse(payload);
            var element = doc.RootElement;

            if (element.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object)
                element = data;
            else if (element.TryGetProperty("meta", out var meta) && meta.ValueKind == JsonValueKind.Object)
                element = meta;

            root = element.Clone();
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryReadString(JsonElement element, string name, out string value)
    {
        value = string.Empty;
        if (!element.TryGetProperty(name, out var prop))
            return false;

        if (prop.ValueKind == JsonValueKind.String)
        {
            value = prop.GetString() ?? string.Empty;
            return !string.IsNullOrWhiteSpace(value);
        }

        value = prop.ToString();
        return !string.IsNullOrWhiteSpace(value);
    }

    private static bool TryReadInt(JsonElement element, string name, out int value)
    {
        value = 0;
        if (!element.TryGetProperty(name, out var prop))
            return false;

        return prop.ValueKind switch
        {
            JsonValueKind.Number => prop.TryGetInt32(out value),
            JsonValueKind.String => int.TryParse(prop.GetString(), out value),
            _ => false
        };
    }

    private static bool TryReadDouble(JsonElement element, string name, out double value)
    {
        value = 0;
        if (!element.TryGetProperty(name, out var prop))
            return false;

        return prop.ValueKind switch
        {
            JsonValueKind.Number => prop.TryGetDouble(out value),
            JsonValueKind.String => double.TryParse(prop.GetString(), out value),
            _ => false
        };
    }

    private static bool TryReadArrayLength(JsonElement element, string name, out int count)
    {
        count = 0;
        if (!element.TryGetProperty(name, out var prop))
            return false;
        if (prop.ValueKind != JsonValueKind.Array)
            return false;

        count = prop.GetArrayLength();
        return true;
    }

    private Task EmitAsync(string eventName, string payload = "")
    {
        if (_server == null)
            return Task.CompletedTask;
        if (_clientId == null)
            return _server.BroadcastAsync(eventName, payload);

        return _server.EmitToClientAsync(_clientId.Value, eventName, payload);
    }

    private void SimulateMetrics()
    {
        StatementsCount += _rng.Next(5, 13);
        GameObjectsCount += _rng.Next(0, 6);
        ScoreProgressValue = Math.Min(100, ScoreProgressValue + _rng.NextDouble() * 2.5);

        var fpsBase = Fps <= 0 ? 70 : Fps;
        Fps = Clamp(fpsBase + (_rng.NextDouble() * 6 - 3), 45, 90);

        var heartBase = HeartRate <= 0 ? 78 : HeartRate;
        HeartRate = (int)Math.Round(Clamp(heartBase + _rng.Next(-4, 5), 60, 140));
    }

    private static double Clamp(double value, double min, double max)
        => value < min ? min : value > max ? max : value;

    private void StartTracking()
    {
        _ = EmitAsync("clients:tracking:start");
        IsTrackingRunning = true;
        IsTrackingPaused = false;
    }
}
