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

    public bool IsMandatory => Plugin is ICorePlugin;
    
    public string Id => Plugin.Id;
    public string Name => Plugin.Name;
    public string Description => Plugin.Description ?? "";
    public string Website => Plugin.Website;
    public string Repository => Plugin.Repository;
    public string ChangelogUrl => Plugin.ChangelogUrl;
    public string Version => Plugin.GetVersion();
    public string FilePath => Plugin.GetFilePath();
    
    public int GridColumns { get; set; }
    public int GridRows { get; set; }

    // --- grouped components
    public ObservableCollection<IVisualization> Visualizations { get; }
    public ObservableCollection<IFilter> Filters { get; }
    public ObservableCollection<IEndpoint> Endpoints { get; }

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

    // --- Icon helpers (Material.Icons.Avalonia uses this enum)
    public MaterialIconKind EnabledIconKind => IsEnabled ? MaterialIconKind.ToggleSwitch : MaterialIconKind.ToggleSwitchOffOutline;
    public string EnabledToolTip => IsEnabled ? "Disable plugin" : "Enable plugin";

    public MaterialIconKind DetailsIconKind => IsExpanded ? MaterialIconKind.ChevronUp : MaterialIconKind.ChevronDown;

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

        Visualizations = new ObservableCollection<IVisualization>(
            types.Where(t => typeof(IVisualization).IsAssignableFrom(t))
                 .Select(t => (IVisualization)Activator.CreateInstance(t!)!)
                 .OrderBy(x => x.Name));

        Filters = new ObservableCollection<IFilter>(
            types.Where(t => typeof(IFilter).IsAssignableFrom(t))
                .Select(t => (IFilter)Activator.CreateInstance(t!)!)
                 .OrderBy(x => x.Name));

        Endpoints = new ObservableCollection<IEndpoint>(
            types.Where(t => typeof(IEndpoint).IsAssignableFrom(t))
                .Select(t => (IEndpoint)Activator.CreateInstance(t!)!)
                 .OrderBy(x => x.Name));

        // Core: disabled by default AND cannot be changed
        _isEnabled = initialEnabled;

        ToggleExpandCommand = new RelayCommand(() => IsExpanded = !IsExpanded);

        ToggleEnabledCommand = new RelayCommand(() =>
        {
            IsEnabled = !IsEnabled; // setter triggers store + icon updates
        });

        RemoveCommand = new RelayCommand(() =>
        {
            _owner.RemovePlugin(this);
        });
    }
}