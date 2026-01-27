using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ReCoPa.Plugins;

namespace ReCoPa.ViewModels;

public sealed class PluginItemViewModel : ViewModelBase
{
    private readonly PluginManagerViewModel _owner;
    private bool _isExpanded;
    private bool _isEnabled;

    public IPluginPackage Plugin { get; }

    public string Id => Plugin.Id;
    public string Name => Plugin.Name;
    public string Description => Plugin.Description ?? "";
    public string Version => Plugin.GetVersion();
    public string FilePath => Plugin.GetFilePath();

    public ObservableCollection<string> Components { get; }
    public ObservableCollection<Contributor> Contributors { get; }

    public bool HasComponents => Components.Count > 0;
    public bool HasContributors => Contributors.Count > 0;

    // --- UI summaries
    public string ComponentSummary => string.Join(" • ", Components);
    public string ContributorSummary => string.Join(" • ", Contributors.Select(c => c.Name));

    // --- Expand/collapse
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
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
            }
        }
    }

    public string EnabledLabel => IsEnabled ? "Enabled" : "Disabled";

    // Core plugin: cannot be changed, and should be disabled by default
    public bool IsCorePlugin => _owner.IsCorePluginId(Id);
    public bool CanToggleEnabled => !IsCorePlugin;
    public bool CanRemove => !IsCorePlugin;

    public ICommand ToggleExpandCommand { get; }
    public ICommand ToggleEnabledCommand { get; }
    public ICommand RemoveCommand { get; }

    public PluginItemViewModel(PluginManagerViewModel owner, IPluginPackage plugin, bool initialEnabled)
    {
        _owner = owner;
        Plugin = plugin;

        Components = new ObservableCollection<string>(
            plugin.Components?.Select(c => c.GetType().Name).Distinct().OrderBy(x => x)
            ?? Enumerable.Empty<string>());

        Contributors = new ObservableCollection<Contributor>(plugin.Contributors ?? Array.Empty<Contributor>());

        // Core: disabled by default AND cannot be changed
        _isEnabled = IsCorePlugin ? false : initialEnabled;

        ToggleExpandCommand = new RelayCommand(() => IsExpanded = !IsExpanded);

        ToggleEnabledCommand = new RelayCommand(() =>
        {
            if (!CanToggleEnabled) return;
            IsEnabled = !IsEnabled;
            OnPropertyChanged(nameof(IsEnabled));
            OnPropertyChanged(nameof(EnabledLabel));
        });

        RemoveCommand = new RelayCommand(() =>
        {
            if (!CanRemove) return;
            _owner.RemovePlugin(this);
        });
    }
}