using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ReCoPa.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    public ObservableCollection<TabViewModel> ClientTabs { get; } = new();

    // ---- Selected client summary for Sidebar ----
    [ObservableProperty] private string? selectedClientName;
    [ObservableProperty] private bool isSelectedClientConnected;

    [ObservableProperty] private int _selectedClientStatementsCount;
    [ObservableProperty] private int selectedClientGameObjectsCount;

    [ObservableProperty] private double selectedClientFps;
    [ObservableProperty] private int selectedClientHeartRate;

    // Optional: which view is currently shown in the main area
    [ObservableProperty] private string currentView = "Visualizations"; // or "Settings"

    public bool IsSelectedClientDisconnected => !IsSelectedClientConnected;
    
    public DashboardViewModel()
    {
        // Demo data (replace with real clients later)
        ClientTabs.Add(new TabViewModel { Header = "VR Training - PC1", IsActive = true });
        ClientTabs.Add(new TabViewModel { Header = "Cognitive Test - Lab-PC", IsActive = false });
        ClientTabs.Add(new TabViewModel { Header = "VR Experience - PC2", IsActive = false });

        ApplySelectedFromActiveTab();

        // Demo numbers
        SelectedClientStatementsCount = 12541;
        SelectedClientGameObjectsCount = 10;
        SelectedClientFps = 72.4;
        SelectedClientHeartRate = 98;
        IsSelectedClientConnected = true;
    }

    private void ApplySelectedFromActiveTab()
    {
        var active = ClientTabs.FirstOrDefault(t => t.IsActive) ?? ClientTabs.FirstOrDefault();
        SelectedClientName = active?.Header ?? "No Session";
    }

    // ---- Commands ----

    [RelayCommand]
    private void AddClient()
    {
        var idx = ClientTabs.Count + 1;
        ClientTabs.Add(new TabViewModel { Header = $"Session {idx}", IsActive = false });
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
        foreach (var t in ClientTabs) t.IsActive = false;
        tab.IsActive = true;

        SelectedClientName = tab.Header;
        // TODO: load counts/fps/heartrate from this tab's client model
    }
    
    [RelayCommand]
    private void SelectTab(TabViewModel? tab)
    {
        if (tab is null) return;

        foreach (var t in ClientTabs)
            t.IsActive = false;

        tab.IsActive = true;

        // optional: hier deine SelectedClient* Felder updaten
        SelectedClientName = tab.Header;
    }
}