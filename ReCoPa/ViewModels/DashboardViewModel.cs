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
    [ObservableProperty] private TabViewModel? activeTab;
    public object? SelectedSessionView => ActiveTab?.SessionView;
    public bool HasSelectedSessionView => SelectedSessionView != null;
    
    public DashboardViewModel(SocketServerHost? server = null)
    {
        _server = server;
        SessionTabs.CollectionChanged += OnSessionTabsChanged;

        RestoreSavedSessions();
        _server!.ClientConnected += OnClientConnected;
        _server!.ClientDisconnected += OnClientDisconnected;
        _server!.ClientHello += OnClientHello;

        UpdateHasSessions();
        ApplySelectedFromActiveTab();
    }

    private void ApplySelectedFromActiveTab()
    {
        var active = SessionTabs.FirstOrDefault(t => t.IsActive) ?? SessionTabs.FirstOrDefault();
        if (active != null)
            SetActiveTab(active);
        else
            ActiveTab = null;
    }

    private void SetActiveTab(TabViewModel tab)
    {
        foreach (var t in SessionTabs) t.IsActive = false;
        tab.IsActive = true;
        ActiveTab = tab;
    }

    private void AddSessionTab(string header, Guid? clientId, TabConnectionState connectionState, bool activate)
    {
        var session = new SessionViewModel(header, _server, clientId)
        {
            IsConnected = connectionState == TabConnectionState.Connected
        };
        session.CurrentView = "Settings";
        CreateTab(session, header, clientId, connectionState, activate);
    }

    private void RestoreSavedSessions()
    {
        var snapshots = SessionStore.LoadSessions();
        foreach (var snapshot in snapshots)
        {
            var session = new SessionViewModel(snapshot.Name, _server, clientId: null);
            session.ApplySnapshot(snapshot);

            var tab = CreateTab(
                session,
                session.ClientName ?? snapshot.Name,
                clientId: null,
                connectionState: TabConnectionState.Disconnected,
                activate: snapshot.IsActive);

            if (snapshot.IsActive)
                SetActiveTab(tab);
        }
    }

    private TabViewModel CreateTab(SessionViewModel session, string header, Guid? clientId,
        TabConnectionState connectionState, bool activate)
    {
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

        return tab;
    }

    private void OnSessionTabsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateHasSessions();

        if (!HasSessions)
        {
            ActiveTab = null;
            return;
        }

        var activeTabIsValid = ActiveTab != null && SessionTabs.Contains(ActiveTab);
        var selectedViewIsValid = ActiveTab?.SessionView != null;

        if (!SessionTabs.Any(t => t.IsActive) || !activeTabIsValid || !selectedViewIsValid)
            ApplySelectedFromActiveTab();
    }

    private void UpdateHasSessions()
    {
        HasSessions = SessionTabs.Count > 0;
    }

    private void OnClientConnected(SocketServerHost.SocketConnection conn)
    {
        var waitingTab = SessionTabs.FirstOrDefault(t => t.ClientId == null && t.ConnectionState == TabConnectionState.Inactive);
        if (waitingTab != null)
        {
            waitingTab.ClientId = conn.Id;
            waitingTab.ConnectionState = TabConnectionState.Connected;
            if (waitingTab.Session != null)
            {
                waitingTab.Session.ClientId = conn.Id;
                waitingTab.Session.IsConnected = true;
            }
            SetActiveTab(waitingTab);

            return;
        }

        var header = $"Session {SessionTabs.Count + 1}";

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

    private void OnClientHello(SocketServerHost.SocketConnection conn, ClientHello hello)
    {
        Console.WriteLine("Info " + hello);
        if (string.IsNullOrWhiteSpace(hello.SessionId))
            return;

        var tab = SessionTabs.FirstOrDefault(t => t.ClientId == conn.Id);
        if (tab?.Session == null)
            return;

      
        tab.Session.ClientName = hello.SessionId;
        tab.Header = hello.SessionId;
    }

    // ---- Commands ----

    [RelayCommand]
    private void AddClient()
    {
        var idx = SessionTabs.Count + 1;
        AddSessionTab($"Session {idx}", clientId: null, connectionState: TabConnectionState.Inactive, activate: true);
        ApplySelectedFromActiveTab();
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

        if (wasActive)
            ActiveTab = null;

        if (tab.Session != null && _sessionNameHandlers.TryGetValue(tab.Session, out var handler))
        {
            tab.Session.PropertyChanged -= handler;
            _sessionNameHandlers.Remove(tab.Session);
        }

        if (tab.Session != null)
        {
            SessionStore.SaveSession(tab.Session);
            SessionStore.MarkSessionClosed(tab.Session.SessionId);
            tab.Session.Dispose();
        }
        SessionTabs.Remove(tab);

        if (!wasActive || SessionTabs.Count == 0)
            return;

        var nextIndex = Math.Min(index, SessionTabs.Count - 1);
        SetActiveTab(SessionTabs[nextIndex]);
    }

    partial void OnActiveTabChanged(TabViewModel? value)
    {
        OnPropertyChanged(nameof(SelectedSessionView));
        OnPropertyChanged(nameof(HasSelectedSessionView));
    }
}
