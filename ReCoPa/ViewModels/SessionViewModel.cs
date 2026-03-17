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
using ReCoPa.Services;

namespace ReCoPa.ViewModels;

public partial class SessionViewModel : ViewModelBase, IDisposable
{
    private DateTime _startedAtUtc = DateTime.UtcNow;
    public DateTime CreatedUtc { get; private set; } = DateTime.UtcNow;
    private readonly DispatcherTimer _timer;
    private readonly SocketServerHost? _server;
    private readonly List<IDisposable> _subscriptions = new();
    private readonly Random _rng = new();

    public VisualizationContainerViewModel Visualization { get; } = new();
    public SessionSettingsViewModel Settings { get; } = new();

    [ObservableProperty] private string? clientName;
    [ObservableProperty] private Guid sessionId = Guid.NewGuid();
    [ObservableProperty] private Guid? clientId;
    [ObservableProperty] private bool isConnected;
    [ObservableProperty] private int statementsCount;
    [ObservableProperty] private int gameObjectsCount;
    [ObservableProperty] private double fps;
    [ObservableProperty] private int heartRate;
    [ObservableProperty] private double scoreProgressValue;
    [ObservableProperty] private TimeSpan elapsedTime = TimeSpan.Zero;
    [ObservableProperty] private bool isSessionSelected = true;
    [ObservableProperty] private bool isEyeTrackingEnabled = true;
    [ObservableProperty] private bool isTrackingRunning;
    [ObservableProperty] private bool isTrackingPaused;
    [ObservableProperty] private bool isEditingSessionName;
    [ObservableProperty] private string sessionNameEdit = string.Empty;
    [ObservableProperty] private string currentView = "Visualizations";

    public bool IsVisualizationsView => CurrentView == "Visualizations";
    public bool IsSettingsView => CurrentView == "Settings";

    public bool IsAwaitingConnection => !IsConnected && ClientId == null;
    public bool IsDisconnected => !IsConnected && !IsAwaitingConnection;

    public string StartStopText => IsTrackingRunning ? "Stop" : "Start";
    public MaterialIconKind StartStopIcon => IsTrackingRunning ? MaterialIconKind.Stop : MaterialIconKind.PlayCircle;

    public SessionViewModel(string? clientName = null, SocketServerHost? server = null, Guid? clientId = null)
    {
        ClientName = clientName ?? "Session";
        _server = server ?? App.Socket;
        ClientId = clientId;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += OnTick;

        SubscribeToSocket();
    }

    public void ApplySnapshot(SessionSnapshot snapshot)
    {
        if (snapshot == null)
            return;

        SessionId = snapshot.Id == Guid.Empty ? Guid.NewGuid() : snapshot.Id;
        ClientId = null;
        IsConnected = false;
        ClientName = string.IsNullOrWhiteSpace(snapshot.Name) ? ClientName : snapshot.Name;
        CreatedUtc = snapshot.CreatedUtc == default ? DateTime.UtcNow : snapshot.CreatedUtc;

        IsEyeTrackingEnabled = snapshot.IsEyeTrackingEnabled;
        IsTrackingRunning = snapshot.IsTrackingRunning;
        IsTrackingPaused = snapshot.IsTrackingPaused;

        StatementsCount = snapshot.StatementsCount;
        GameObjectsCount = snapshot.GameObjectsCount;
        Fps = snapshot.Fps;
        HeartRate = snapshot.HeartRate;
        ScoreProgressValue = snapshot.ScoreProgressValue;

        CurrentView = string.IsNullOrWhiteSpace(snapshot.CurrentView) ? "Visualizations" : snapshot.CurrentView;

        var elapsed = TimeSpan.FromSeconds(Math.Max(0, snapshot.ElapsedSeconds));
        _startedAtUtc = DateTime.UtcNow - elapsed;
        ElapsedTime = elapsed;

        Settings.ApplySnapshot(snapshot.Settings);

        if (snapshot.GridRows > 0)
            Visualization.GridRows = snapshot.GridRows;
        if (snapshot.GridColumns > 0)
            Visualization.GridColumns = snapshot.GridColumns;
        Visualization.RestoreFromSnapshots(snapshot.Visualizations);
    }

    [RelayCommand]
    private void NavigateVisualizations() => CurrentView = "Visualizations";

    [RelayCommand]
    private void NavigateSettings() => CurrentView = "Settings";

    [RelayCommand]
    private void StartCalibration()
    {
        _ = EmitAsync("calibration:start");
    }

    [RelayCommand]
    private void PauseTracking()
    {
        if (!IsTrackingRunning)
            return;

        _ = EmitAsync("tracking:pause");
        IsTrackingPaused = true;
        _timer.Stop();
    }

    [RelayCommand]
    private void StopTracking()
    {
        _ = EmitAsync("tracking:stop");
        IsTrackingRunning = false;
        IsTrackingPaused = false;
        _timer.Stop();
    }

    [RelayCommand]
    private void StartStopTracking()
    {
        if (IsTrackingRunning)
        {
            StopTracking();
            return;
        }

        StartTracking();
    }

    [RelayCommand]
    private void StartEditSessionName()
    {
        SessionNameEdit = ClientName ?? string.Empty;
        IsEditingSessionName = true;
    }

    [RelayCommand]
    private void SaveSessionName()
    {
        var name = SessionNameEdit?.Trim();
        if (!string.IsNullOrWhiteSpace(name))
            ClientName = name;

        IsEditingSessionName = false;
    }

    [RelayCommand]
    private void CancelSessionNameEdit()
    {
        SessionNameEdit = ClientName ?? string.Empty;
        IsEditingSessionName = false;
    }

    [RelayCommand]
    private void ShutdownApp()
    {
        _ = EmitAsync("shutdown");
    }

    partial void OnIsConnectedChanged(bool value)
    {
        OnPropertyChanged(nameof(IsDisconnected));
        OnPropertyChanged(nameof(IsAwaitingConnection));
    }

    partial void OnClientIdChanged(Guid? value)
    {
        OnPropertyChanged(nameof(IsDisconnected));
        OnPropertyChanged(nameof(IsAwaitingConnection));
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

        if (!IsConnected && IsTrackingRunning)
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

        _subscriptions.Add(_server.On("info", HandleMeta));
        _subscriptions.Add(_server.On("meta", HandleMeta));
        _subscriptions.Add(_server.On("statements", HandleStatement));
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
        else if (TryReadDouble(root, "heartRate", out var heartRateDouble))
            HeartRate = (int)Math.Round(heartRateDouble);
        if (TryReadDouble(root, "fps", out var fps))
            Fps = fps;
        if (TryReadDouble(root, "score", out var score))
            ScoreProgressValue = score;

        if (TryReadBool(root, "isTracking", out var isTracking))
            IsTrackingRunning = isTracking;
        if (TryReadBool(root, "isTrackingPaused", out var isTrackingPaused))
            IsTrackingPaused = isTrackingPaused;
        if (TryReadBool(root, "isCalibrated", out var isCalibrated))
            IsEyeTrackingEnabled = isCalibrated;
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
        if (ClientId == null)
            return true;

        if (TryReadString(root, "clientId", out var idText)
            || TryReadString(root, "client_id", out idText)
            || TryReadString(root, "id", out idText))
        {
            if (Guid.TryParse(idText, out var id))
                return id == ClientId;
        }

        if (root.TryGetProperty("client", out var clientEl)
            && TryReadString(clientEl, "id", out idText)
            && Guid.TryParse(idText, out var clientId))
        {
            return clientId == ClientId;
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
            else if (element.ValueKind == JsonValueKind.String)
            {
                var nested = element.GetString();
                if (!string.IsNullOrWhiteSpace(nested))
                {
                    using var nestedDoc = JsonDocument.Parse(nested);
                    element = nestedDoc.RootElement.Clone();
                }
            }

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

    private static bool TryReadBool(JsonElement element, string name, out bool value)
    {
        value = false;
        if (!element.TryGetProperty(name, out var prop))
            return false;

        return prop.ValueKind switch
        {
            JsonValueKind.True => (value = true) == true,
            JsonValueKind.False => (value = false) == false,
            JsonValueKind.String => bool.TryParse(prop.GetString(), out value),
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
        if (ClientId == null)
            return _server.BroadcastAsync(eventName, payload);

        return _server.EmitToClientAsync(ClientId.Value, eventName, payload);
    }

    private static double Clamp(double value, double min, double max)
        => value < min ? min : value > max ? max : value;

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

    private void StartTracking()
    {
        _ = EmitAsync("tracking:start");
        IsTrackingRunning = true;
        IsTrackingPaused = false;
        _timer.Start();
    }
}
