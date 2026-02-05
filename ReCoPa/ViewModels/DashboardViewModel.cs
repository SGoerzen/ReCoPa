using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReCoPa.Network;
using ReCoPa.Services;

namespace ReCoPa.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly SocketServerHost? _server;
    private readonly Dictionary<SessionViewModel, System.ComponentModel.PropertyChangedEventHandler> _sessionNameHandlers = new();

    public ObservableCollection<TabViewModel> SessionTabs { get; } = new();

    [ObservableProperty] private bool hasSessions;
    [ObservableProperty] private object? selectedSessionView;
    
    public DashboardViewModel(SocketServerHost? server = null)
    {
        _server = server;
        SessionTabs.CollectionChanged += OnSessionTabsChanged;

        if (_server == null)
        {
            AddSessionTab("VR Training - PC1", Guid.NewGuid(), TabConnectionState.Connected, activate: true);
            AddSessionTab("Cognitive Test - Lab-PC", Guid.NewGuid(), TabConnectionState.Disconnected, activate: false);
            AddSessionTab("VR Experience - PC2", Guid.NewGuid(), TabConnectionState.Connected, activate: false);
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
        SelectedSessionView = active?.SessionView;
    }

    private void SetActiveTab(TabViewModel tab)
    {
        foreach (var t in SessionTabs) t.IsActive = false;
        tab.IsActive = true;
        SelectedSessionView = tab.SessionView;
    }

    private void AddSessionTab(string header, Guid? clientId, TabConnectionState connectionState, bool activate)
    {
        var session = new SessionViewModel(header, _server, clientId)
        {
            IsConnected = connectionState == TabConnectionState.Connected
        };
        session.CurrentView = "Settings";
        var sessionView = new Views.SessionView
        {
            DataContext = session
        };

        var tab = new TabViewModel
        {
            Header = header,
            IsActive = false,
            ConnectionState = connectionState,
            ClientId = clientId,
            Session = session,
            SessionView = sessionView
        };

        var handler = new System.ComponentModel.PropertyChangedEventHandler((_, e) =>
        {
            if (e.PropertyName == nameof(SessionViewModel.ClientName) && !string.IsNullOrWhiteSpace(session.ClientName))
                tab.Header = session.ClientName;
        });
        session.PropertyChanged += handler;
        _sessionNameHandlers[session] = handler;

        SessionTabs.Add(tab);

        if (activate || SessionTabs.Count == 1)
            SetActiveTab(tab);
    }

    private void OnSessionTabsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateHasSessions();

        if (!HasSessions)
        {
            SelectedSessionView = null;
            return;
        }

        if (!SessionTabs.Any(t => t.IsActive))
            ApplySelectedFromActiveTab();
    }

    private void UpdateHasSessions()
    {
        HasSessions = SessionTabs.Count > 0;
    }

    private void OnClientConnected(SocketServerHost.SocketConnection conn)
    {
        var waitingTab = SessionTabs.FirstOrDefault(t => t.ClientId == null);
        if (waitingTab != null)
        {
            waitingTab.ClientId = conn.Id;
            waitingTab.ConnectionState = TabConnectionState.Connected;
            if (waitingTab.Session != null)
            {
                waitingTab.Session.ClientId = conn.Id;
                waitingTab.Session.IsConnected = true;
            }

            return;
        }

        var endpoint = conn.RemoteEndPoint?.ToString();
        var header = string.IsNullOrWhiteSpace(endpoint)
            ? $"Session {SessionTabs.Count + 1}"
            : $"Session {SessionTabs.Count + 1} ({endpoint})";

        AddSessionTab(header, conn.Id, TabConnectionState.Connected, activate: SessionTabs.Count == 0);
        SukiDialogService.ShowInfoToast("Neue Session", "Neue Session wurde automatisch eröffnet.");
    }

    private void OnClientDisconnected(SocketServerHost.SocketConnection conn)
    {
        var tab = SessionTabs.FirstOrDefault(t => t.ClientId == conn.Id);
        if (tab != null)
        {
            tab.ConnectionState = TabConnectionState.Disconnected;
            if (tab.Session != null)
                tab.Session.IsConnected = false;
        }
    }

    // ---- Commands ----

    [RelayCommand]
    private void AddClient()
    {
        var idx = SessionTabs.Count + 1;
        AddSessionTab($"Session {idx}", clientId: null, connectionState: TabConnectionState.Inactive, activate: true);
    }

    [RelayCommand]
    private void SelectTab(TabViewModel? tab)
    {
        if (tab is null) return;

        SetActiveTab(tab);
    }

    [RelayCommand]
    private async Task CloseTab(TabViewModel? tab)
    {
        if (tab is null) return;

        var confirm = await SukiDialogService.ConfirmSessionCloseAsync();
        if (!confirm) return;

        var index = SessionTabs.IndexOf(tab);
        var wasActive = tab.IsActive;

        if (tab.Session != null && _sessionNameHandlers.TryGetValue(tab.Session, out var handler))
        {
            tab.Session.PropertyChanged -= handler;
            _sessionNameHandlers.Remove(tab.Session);
        }

        tab.Session?.Dispose();
        SessionTabs.Remove(tab);

        if (!wasActive || SessionTabs.Count == 0)
            return;

        var nextIndex = Math.Min(index, SessionTabs.Count - 1);
        SetActiveTab(SessionTabs[nextIndex]);
    }
}
