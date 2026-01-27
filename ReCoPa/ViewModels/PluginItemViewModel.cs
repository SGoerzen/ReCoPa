using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ReCoPa.Plugins;

using Material.Icons;

namespace ReCoPa.ViewModels;

public sealed class PluginItemViewModel : ViewModelBase
{
    private readonly PluginManagerViewModel _owner;
    private bool _isEnabled;

    public IPluginPackage Plugin { get; }

    public string Id => Plugin.Id;
    public string Name => Plugin.Name;
    public string Description => Plugin.Description ?? "";
    public string Version => Plugin.GetVersion();
    public string FilePath => Plugin.GetFilePath();
    
    public int GridColumns { get; set; }
    public int GridRows { get; set; }

    // --- grouped components
    public ObservableCollection<string> Visualizations { get; }
    public ObservableCollection<string> Filters { get; }
    public ObservableCollection<string> Endpoints { get; }

    public bool HasVisualizations => Visualizations.Count > 0;
    public bool HasFilters => Filters.Count > 0;
    public bool HasEndpoints => Endpoints.Count > 0;

    public ObservableCollection<Contributor> Contributors { get; }
    public bool HasContributors => Contributors.Count > 0;

    // --- UI summaries
    public string ComponentSummary
    {
        get
        {
            var parts = new[]
            {
                HasVisualizations ? $"Visualizations: {Visualizations.Count}" : null,
                HasFilters ? $"Filters: {Filters.Count}" : null,
                HasEndpoints ? $"Endpoints: {Endpoints.Count}" : null
            }.Where(x => x is not null);

            return string.Join(" • ", parts!);
        }
    }

    public string ContributorSummary => string.Join(" • ", Contributors.Select(c => c.Name));

    // --- Expand/collapse
    public bool IsExpanded
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
                OnPropertyChanged(nameof(DetailsIconKind));
        }
    }

    // --- Enabled/disabled
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            // Core: cannot change, always false
            if (IsCorePlugin)
            {
                if (_isEnabled != false)
                {
                    _isEnabled = false;
                    OnPropertyChanged();
                }
                return;
            }

            if (SetProperty(ref _isEnabled, value))
            {
                _owner.SetEnabled(this, value);
                OnPropertyChanged(nameof(EnabledLabel));
                OnPropertyChanged(nameof(EnabledIconKind));
                OnPropertyChanged(nameof(EnabledToolTip));
            }
        }
    }

    public string EnabledLabel => IsEnabled ? "Enabled" : "Disabled";

    // --- Core plugin rules
    public bool IsCorePlugin => _owner.IsCorePluginId(Id);

    // For UI visibility (mandatory requirement: Core shows ONLY Details)
    public bool ShowToggle => !IsCorePlugin;
    public bool ShowRemove => !IsCorePlugin;

    // --- Icon helpers (Material.Icons.Avalonia uses this enum)
    public MaterialIconKind EnabledIconKind => IsEnabled ? MaterialIconKind.ToggleSwitch : MaterialIconKind.ToggleSwitchOffOutline;
    public string EnabledToolTip => IsEnabled ? "Disable plugin" : "Enable plugin";

    public MaterialIconKind DetailsIconKind => IsExpanded ? MaterialIconKind.ChevronUp : MaterialIconKind.ChevronDown;

    // Old flags still usable if you prefer IsEnabled binding
    public bool CanToggleEnabled => !IsCorePlugin;
    public bool CanRemove => !IsCorePlugin;

    public ICommand ToggleExpandCommand { get; }
    public ICommand ToggleEnabledCommand { get; }
    public ICommand RemoveCommand { get; }

    public PluginItemViewModel(PluginManagerViewModel owner, IPluginPackage plugin, bool initialEnabled, int gridColumns, int gridRows)
    {
        _owner = owner;
        Plugin = plugin;
        GridColumns = gridColumns;
        GridRows = gridRows;

        // Contributors
        Contributors = new ObservableCollection<Contributor>(plugin.Contributors ?? Array.Empty<Contributor>());

        // Component grouping (IVisualization / IFilter / IEndpoint)
        var types = (plugin.Components ?? Array.Empty<object>())
            .Select(c => c?.GetType())
            .Where(t => t is not null)!
            .Distinct()
            .ToArray();

        Visualizations = new ObservableCollection<string>(
            types.Where(t => typeof(IVisualization).IsAssignableFrom(t))
                 .Select(t => t.Name)
                 .OrderBy(x => x));

        Filters = new ObservableCollection<string>(
            types.Where(t => typeof(IFilter).IsAssignableFrom(t))
                 .Select(t => t.Name)
                 .OrderBy(x => x));

        Endpoints = new ObservableCollection<string>(
            types.Where(t => typeof(IEndpoint).IsAssignableFrom(t))
                 .Select(t => t.Name)
                 .OrderBy(x => x));

        // Core: disabled by default AND cannot be changed
        _isEnabled = IsCorePlugin ? false : initialEnabled;

        ToggleExpandCommand = new RelayCommand(() => IsExpanded = !IsExpanded);

        ToggleEnabledCommand = new RelayCommand(() =>
        {
            if (!CanToggleEnabled) return;
            IsEnabled = !IsEnabled; // setter triggers store + icon updates
        });

        RemoveCommand = new RelayCommand(() =>
        {
            if (!CanRemove) return;
            _owner.RemovePlugin(this);
        });
    }
}