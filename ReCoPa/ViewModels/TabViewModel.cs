using System;
using ReactiveUI;

namespace ReCoPa.ViewModels;

public class TabViewModel : ReactiveObject
{
    private string _header = string.Empty;
    private bool _isActive;

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
}
