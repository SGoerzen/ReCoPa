// ViewModels/SettingsViewModel.cs
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReCoPa.Services;

namespace ReCoPa.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    public ObservableCollection<EndpointViewModel> Endpoints { get; } = new();
    public ObservableCollection<ToggleChipViewModel> TrackingToggles { get; } = new();

    public ObservableCollection<string> ExcludeGameObjects { get; } = new();
    public ObservableCollection<string> ExcludeComponents { get; } = new();
    public ObservableCollection<string> ExcludeGestures { get; } = new();

    [ObservableProperty] private string? filterSearchText;
    [ObservableProperty] private string? unityManifestStatus;
    [ObservableProperty] private string? unityManifestPath;

    public SettingsViewModel()
    {
        Endpoints.Add(new EndpointViewModel
        {
            Name = "Local CSV",
            Type = "File",
            Target = "ReCoPa/exports/statements.csv",
            IsEnabled = true
        });
        Endpoints.Add(new EndpointViewModel
        {
            Name = "LRSolid",
            Type = "LRS",
            Target = "https://lrs.example.org/xapi",
            IsEnabled = false
        });

        TrackingToggles.Add(new ToggleChipViewModel("Head Pose"));
        TrackingToggles.Add(new ToggleChipViewModel("Hands"));
        TrackingToggles.Add(new ToggleChipViewModel("Eye Tracking"));
        TrackingToggles.Add(new ToggleChipViewModel("Controllers"));
        TrackingToggles.Add(new ToggleChipViewModel("Collisions"));

        ExcludeGameObjects.Add("XR Rig");
        ExcludeGameObjects.Add("DebugCanvas");
        ExcludeGameObjects.Add("UIRoot");

        ExcludeComponents.Add("AudioSource");
        ExcludeComponents.Add("Animator");
        ExcludeComponents.Add("MeshRenderer");

        ExcludeGestures.Add("Swipe");
        ExcludeGestures.Add("Pinch");
        ExcludeGestures.Add("Teleport");
    }

    [RelayCommand]
    private void AddEndpoint()
    {
        Endpoints.Add(new EndpointViewModel
        {
            Name = $"Endpoint {Endpoints.Count + 1}",
            Type = "LRS",
            Target = "",
            IsEnabled = true
        });
    }

    [RelayCommand]
    private void RemoveEndpoint(EndpointViewModel? endpoint)
    {
        if (endpoint != null)
            Endpoints.Remove(endpoint);
    }

    [RelayCommand]
    private void ApplySettings()
    {
        // TODO: persist/apply to runtime
    }

    public async Task UpdateUnityManifestAsync(string manifestPath)
    {
        UnityManifestPath = manifestPath;
        UnityManifestStatus = await UnityManifestUpdater.UpdateAsync(manifestPath);
    }
}

public partial class EndpointViewModel : ObservableObject
{
    [ObservableProperty] private string name = "";
    [ObservableProperty] private string type = "LRS";
    [ObservableProperty] private string target = "";
    [ObservableProperty] private bool isEnabled;
}

public partial class ToggleChipViewModel : ObservableObject
{
    public ToggleChipViewModel(string title) => Title = title;

    public string Title { get; }

    [ObservableProperty] private bool isOn = true;
}
