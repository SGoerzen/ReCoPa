using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using ReCoPa;
using ReCoPa.Models;
using ReCoPa.ViewModels;

public class SidebarViewModel : ViewModelBase
{
    private bool _isTrackingRunning;
    private bool _isTrackingPaused;

    public SidebarViewModel()
    {
        App.Socket?.On<TrackingMeta>("info", (meta) =>
        {
            Console.WriteLine(meta);
            IsTrackingRunning = meta.isTracking;
            IsTrackingPaused = meta.isTrackingPaused;
            IsEyeTrackingEnabled = meta.isCalibratable;
            RaiseTrackingUi();
        });
    }

    public bool IsTrackingRunning
    {
        get => _isTrackingRunning;
        set => SetProperty(ref _isTrackingRunning, value);
    }

    public bool IsTrackingPaused
    {
        get => _isTrackingPaused;
        set => SetProperty(ref _isTrackingPaused, value);
    }

    public bool IsEyeTrackingEnabled { get; set; } = false;

    // === Pause / Resume ===
    public string PauseResumeText =>
        IsTrackingPaused ? "Resume Tracking" : "Pause Tracking";

    public MaterialIconKind PauseResumeIcon =>
        IsTrackingPaused ? MaterialIconKind.Play : MaterialIconKind.Pause;

    // === Start / Stop ===
    public string StartStopText =>
        IsTrackingRunning ? "Stop Tracking" : "Start Tracking";

    public MaterialIconKind StartStopIcon =>
        IsTrackingRunning ? MaterialIconKind.Stop : MaterialIconKind.PlayCircle;

    public ICommand TogglePauseCommand => new RelayCommand(() =>
    {
        if (!IsTrackingRunning) return;

        IsTrackingPaused = !IsTrackingPaused;
        RaiseTrackingUi();
    });

    public ICommand ToggleStartStopCommand => new RelayCommand(() =>
    {
        IsTrackingRunning = !IsTrackingRunning;
        IsTrackingPaused = false;
        RaiseTrackingUi();
    });

    private void RaiseTrackingUi()
    {
        OnPropertyChanged(nameof(PauseResumeText));
        OnPropertyChanged(nameof(PauseResumeIcon));
        OnPropertyChanged(nameof(StartStopText));
        OnPropertyChanged(nameof(StartStopIcon));
    }
}
