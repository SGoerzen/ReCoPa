using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReCoPa.Plugins;

namespace ReCoPa.ViewModels;

public partial class VisualizationContainerViewModel : ObservableObject
{
    public ObservableCollection<VisualizationHostItem> Views { get; } = new();
    public ObservableCollection<IVisualization> AvailableVisualizations { get; } = new();

    [ObservableProperty] private IVisualization? selectedToAdd;

    public ObservableCollection<string> LayoutOptions { get; } = new()
    {
        "1:1", "2:1", "1:2", "2:2", "3:1", "1:3"
    };

    [ObservableProperty] private string selectedLayout = "2:2";
    [ObservableProperty] private int gridRows = 2;

    private int _gridColumns = 2;
    public int GridColumns
    {
        get => _gridColumns;
        set => SetProperty(ref _gridColumns, value);
    }

    public IRelayCommand AddVisualizationCommand { get; }
    public IRelayCommand<VisualizationHostItem> RemoveVisualizationCommand { get; }
    public IRelayCommand<VisualizationHostItem> ToggleSettingsCommand { get; }

    public VisualizationContainerViewModel()
    {
        AddVisualizationCommand = new RelayCommand(AddSelectedVisualization, () => SelectedToAdd is not null);
        RemoveVisualizationCommand = new RelayCommand<VisualizationHostItem>(RemoveVisualization);
        ToggleSettingsCommand = new RelayCommand<VisualizationHostItem>(ToggleSettings);

        ReloadAvailableVisualizations();
        ApplyLayout(SelectedLayout);

        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SelectedLayout))
                ApplyLayout(SelectedLayout);

            if (e.PropertyName == nameof(SelectedToAdd))
                AddVisualizationCommand.NotifyCanExecuteChanged();
        };
    }

    private void RemoveVisualization(VisualizationHostItem? item)
    {
        if (item is null) return;
        Views.Remove(item);
    }

    private void ToggleSettings(VisualizationHostItem? item)
    {
        if (item is null) return;
        item.IsSettingsOpen = !item.IsSettingsOpen;
    }

    private void ReloadAvailableVisualizations()
    {
        AvailableVisualizations.Clear();

        var vis = App.PluginManager?.Visualizations;
        if (vis is null) return;

        foreach (var v in vis)
            AvailableVisualizations.Add(v);

        SelectedToAdd ??= AvailableVisualizations.FirstOrDefault();
    }

    private void AddSelectedVisualization()
    {
        if (SelectedToAdd is null) return;

        var view = CreateViewFromVisualization(SelectedToAdd);
        var settings = CreateSettingsViewFromVisualization(SelectedToAdd);

        Views.Add(new VisualizationHostItem
        {
            View = view,
            Title = SelectedToAdd.Name,
            SettingsView = settings
        });
    }

    private void ApplyLayout(string layout)
    {
        var parts = layout.Split(':');
        if (parts.Length != 2) return;

        if (int.TryParse(parts[0], out var c)) GridColumns = Math.Max(1, c);
        if (int.TryParse(parts[1], out var r)) GridRows = Math.Max(1, r);
    }

    private static Control CreateViewFromVisualization(IVisualization viz)
    {
        object? created;

        try
        {
            created = viz.CreateView();
        }
        catch (Exception ex)
        {
            return new TextBlock
            {
                Text = $"Visualization '{viz.Name}' failed to create view:\n{ex.Message}",
                Opacity = 0.7,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };
        }

        // Host muss ein Avalonia Control rendern.
        if (created is Control control)
            return control;

        return new Border
        {
            Padding = new Avalonia.Thickness(12),
            Child = new TextBlock
            {
                Text =
                    $"Visualization '{viz.Name}' returned '{created?.GetType().FullName ?? "null"}', but a Control is required.\n\n" +
                    "Fix: return an Avalonia Control from CreateView().",
                Opacity = 0.7,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            }
        };
    }

    private static Control CreateSettingsViewFromVisualization(IVisualization viz)
    {
        // Aktuell: Plugin liefert keine Avalonia UI -> Placeholder.
        // (Später kannst du hier z.B. Settings-Schema aus Plugin lesen & UI generieren.)
        return new TextBlock
        {
            Text = "No settings available.",
            Opacity = 0.8,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        };
    }
}

public class VisualizationHostItem : ObservableObject
{
    public required Control View { get; init; }
    public required string Title { get; init; }

    public Control? SettingsView { get; init; }

    public bool HasSettingsView => SettingsView != null;

    private bool _isSettingsOpen;
    public bool IsSettingsOpen
    {
        get => _isSettingsOpen;
        set => SetProperty(ref _isSettingsOpen, value);
    }
}