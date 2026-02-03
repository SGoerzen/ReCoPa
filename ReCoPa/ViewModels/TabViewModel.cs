using System;
using ReactiveUI;

namespace ReCoPa.ViewModels;

public class TabViewModel : ReactiveObject
{
    private string _header = string.Empty;
    private bool _isActive;
    private TabConnectionState _connectionState = TabConnectionState.Inactive;
    private Guid? _clientId;

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
}

public enum TabConnectionState
{
    Inactive,
    Connected,
    Disconnected
}
