using System;
using Avalonia.Controls;
using ReactiveUI;

namespace ReCoPa.ViewModels;

public class TabViewModel : ReactiveObject
{
    private string _header = string.Empty;
    private bool _isActive;
    private TabConnectionState _connectionState = TabConnectionState.Inactive;
    private Guid? _clientId;
    private SessionViewModel? _session;
    private Control? _sessionView;

    public string Header
    {
        get => _header;
        set => this.RaiseAndSetIfChanged(ref _header, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    public TabConnectionState ConnectionState
    {
        get => _connectionState;
        set => this.RaiseAndSetIfChanged(ref _connectionState, value);
    }

    public Guid? ClientId
    {
        get => _clientId;
        set => this.RaiseAndSetIfChanged(ref _clientId, value);
    }

    public SessionViewModel? Session
    {
        get => _session;
        set => this.RaiseAndSetIfChanged(ref _session, value);
    }

    public Control? SessionView
    {
        get => _sessionView;
        set => this.RaiseAndSetIfChanged(ref _sessionView, value);
    }
}

public enum TabConnectionState
{
    Inactive,
    Connected,
    Disconnected
}
