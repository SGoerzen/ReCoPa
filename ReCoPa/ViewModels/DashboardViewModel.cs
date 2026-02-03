using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReCoPa.Network;

namespace ReCoPa.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly SocketServerHost? _server;

    public ObservableCollection<TabViewModel> SessionTabs { get; } = new();

    // ---- Selected client summary for Sidebar ----
    [ObservableProperty] private string? selectedClientName;
    [ObservableProperty] private bool isSelectedClientConnected;

    [ObservableProperty] private int _selectedClientStatementsCount;
    [ObservableProperty] private int selectedClientGameObjectsCount;

    [ObservableProperty] private double selectedClientFps;
    [ObservableProperty] private int selectedClientHeartRate;

    // Optional: which view is currently shown in the main area
    [ObservableProperty] private string currentView = "Visualizations"; // or "Settings"

    [ObservableProperty] private bool hasSessions;
    [ObservableProperty] private bool isSessionSelected;

    public bool IsSelectedClientDisconnected => !IsSelectedClientConnected;
    
    public DashboardViewModel(SocketServerHost? server = null)
    {
        _server = server;
        SessionTabs.CollectionChanged += OnSessionTabsChanged;

        if (_server == null)
        {
            // Demo data (replace with real clients later)
            SessionTabs.Add(new TabViewModel { Header = "VR Training - PC1", IsActive = true, ConnectionState = TabConnectionState.Connected });
            SessionTabs.Add(new TabViewModel { Header = "Cognitive Test - Lab-PC", IsActive = false, ConnectionState = TabConnectionState.Disconnected });
            SessionTabs.Add(new TabViewModel { Header = "VR Experience - PC2", IsActive = false, ConnectionState = TabConnectionState.Connected });

            // Demo numbers
            SelectedClientStatementsCount = 12541;
            SelectedClientGameObjectsCount = 10;
            SelectedClientFps = 72.4;
            SelectedClientHeartRate = 98;
            IsSelectedClientConnected = true;
        }
        else
        {
            _server.ClientConnected += OnClientConnected;
            _server.ClientDisconnected += OnClientDisconnected;
        }

        UpdateHasSessions();
        ApplySelectedFromActiveTab();
    }

    private void ApplySelectedFromActiveTab()
    {
        var active = SessionTabs.FirstOrDefault(t => t.IsActive) ?? SessionTabs.FirstOrDefault();
        SelectedClientName = active?.Header ?? "No Session";
    }

    private void ClearSelectedClientSummary()
    {
        SelectedClientName = "No Session";
        SelectedClientStatementsCount = 0;
        SelectedClientGameObjectsCount = 0;
        SelectedClientFps = 0;
        SelectedClientHeartRate = 0;
        IsSelectedClientConnected = false;
        IsSessionSelected = false;
    }

    private void SetActiveTab(TabViewModel tab)
    {
        foreach (var t in SessionTabs) t.IsActive = false;
        tab.IsActive = true;
        SelectedClientName = tab.Header;
        IsSessionSelected = true;
    }

    private void AddSessionTab(string header, Guid? clientId, TabConnectionState connectionState, bool activate)
    {
        var tab = new TabViewModel
        {
            Header = header,
            IsActive = false,
            ConnectionState = connectionState,
            ClientId = clientId
        };

        SessionTabs.Add(tab);

        if (activate || SessionTabs.Count == 1)
            SetActiveTab(tab);
    }

    private void OnSessionTabsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateHasSessions();

        if (!HasSessions)
        {
            ClearSelectedClientSummary();
            return;
        }

        if (!SessionTabs.Any(t => t.IsActive))
            ApplySelectedFromActiveTab();
    }

    private void UpdateHasSessions()
    {
        HasSessions = SessionTabs.Count > 0;
        IsSessionSelected = HasSessions;
    }

    private void OnClientConnected(SocketServerHost.SocketConnection conn)
    {
        var endpoint = conn.RemoteEndPoint?.ToString();
        var header = string.IsNullOrWhiteSpace(endpoint)
            ? $"Session {SessionTabs.Count + 1}"
            : $"Session {SessionTabs.Count + 1} ({endpoint})";

        AddSessionTab(header, conn.Id, TabConnectionState.Connected, activate: SessionTabs.Count == 0);
    }

    private void OnClientDisconnected(SocketServerHost.SocketConnection conn)
    {
        var tab = SessionTabs.FirstOrDefault(t => t.ClientId == conn.Id);
        if (tab != null)
            tab.ConnectionState = TabConnectionState.Disconnected;
    }

    // ---- Commands ----

    [RelayCommand]
    private void AddClient()
    {
        var idx = SessionTabs.Count + 1;
        AddSessionTab($"Session {idx}", clientId: null, connectionState: TabConnectionState.Inactive, activate: true);
    }

    [RelayCommand]
    private void NavigateVisualizations() => CurrentView = "Visualizations";

    [RelayCommand]
    private void NavigateSettings() => CurrentView = "Settings";

    [RelayCommand]
    private void StartCalibration()
    {
        // TODO: call your calibration logic
        // For now just bump a value to prove it works:
        SelectedClientFps = Math.Max(0, SelectedClientFps - 0.5);
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
        IsSelectedClientConnected = false;
    }

    // Optional: if you want clicking a tab to select it (wire up later)
    public void ActivateTab(TabViewModel tab)
    {
        SetActiveTab(tab);
        // TODO: load counts/fps/heartrate from this tab's client model
    }
    
    [RelayCommand]
    private void SelectTab(TabViewModel? tab)
    {
        if (tab is null) return;

        SetActiveTab(tab);
    }

    [RelayCommand]
    private void CloseTab(TabViewModel? tab)
    {
        if (tab is null) return;

        var index = SessionTabs.IndexOf(tab);
        var wasActive = tab.IsActive;

        SessionTabs.Remove(tab);

        if (!wasActive || SessionTabs.Count == 0)
            return;

        var nextIndex = Math.Min(index, SessionTabs.Count - 1);
        SetActiveTab(SessionTabs[nextIndex]);
    }
}
