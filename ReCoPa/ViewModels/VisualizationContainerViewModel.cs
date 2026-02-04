using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReCoPa.Plugins;
using ReCoPa.Services;

namespace ReCoPa.ViewModels;

public partial class VisualizationContainerViewModel : ObservableObject
{
    public ObservableCollection<VisualizationHostItem> Views { get; } = new();
    public ObservableCollection<IVisualization> AvailableVisualizations { get; } = new();

    [ObservableProperty] private IVisualization? selectedToAdd;
    private int _gridRows = 2;
    private int _gridColumns = 2;

    public int GridRows
    {
        get => _gridRows;
        set => SetProperty(ref _gridRows, Math.Max(1, value));
    }

    public int GridColumns
    {
        get => _gridColumns;
        set => SetProperty(ref _gridColumns, Math.Max(1, value));
    }

    public IRelayCommand AddVisualizationCommand { get; }
    public IAsyncRelayCommand<VisualizationHostItem> RemoveVisualizationCommand { get; }
    public IRelayCommand<VisualizationHostItem> ToggleSettingsCommand { get; }
    public IRelayCommand<VisualizationHostItem> StartTitleEditCommand { get; }
    public IRelayCommand<VisualizationHostItem> SaveTitleEditCommand { get; }
    public IRelayCommand<VisualizationHostItem> CancelTitleEditCommand { get; }

    public VisualizationContainerViewModel()
    {
        AddVisualizationCommand = new RelayCommand(AddSelectedVisualization, () => SelectedToAdd is not null);
        RemoveVisualizationCommand = new AsyncRelayCommand<VisualizationHostItem>(RemoveVisualizationAsync);
        ToggleSettingsCommand = new RelayCommand<VisualizationHostItem>(ToggleSettings);
        StartTitleEditCommand = new RelayCommand<VisualizationHostItem>(StartTitleEdit);
        SaveTitleEditCommand = new RelayCommand<VisualizationHostItem>(SaveTitleEdit);
        CancelTitleEditCommand = new RelayCommand<VisualizationHostItem>(CancelTitleEdit);

        ReloadAvailableVisualizations();

        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SelectedToAdd))
                AddVisualizationCommand.NotifyCanExecuteChanged();
        };
    }

    private async Task RemoveVisualizationAsync(VisualizationHostItem? item)
    {
        if (item is null) return;

        var title = string.IsNullOrWhiteSpace(item.Title) ? "Visualization" : item.Title;
        var confirmed = await SukiDialogService.ConfirmVisualizationDeleteAsync(title);
        if (!confirmed) return;

        Views.Remove(item);
    }

    private void ToggleSettings(VisualizationHostItem? item)
    {
        if (item is null) return;
        item.IsSettingsOpen = !item.IsSettingsOpen;
    }

    private void StartTitleEdit(VisualizationHostItem? item)
    {
        if (item is null) return;
        item.TitleEdit = item.Title;
        item.IsEditingTitle = true;
    }

    private void SaveTitleEdit(VisualizationHostItem? item)
    {
        if (item is null) return;
        var title = item.TitleEdit?.Trim();
        if (!string.IsNullOrWhiteSpace(title))
            item.Title = title;
        item.IsEditingTitle = false;
    }

    private void CancelTitleEdit(VisualizationHostItem? item)
    {
        if (item is null) return;
        item.TitleEdit = item.Title;
        item.IsEditingTitle = false;
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

    public void RefreshAvailableVisualizations()
    {
        ReloadAvailableVisualizations();
        AddVisualizationCommand.NotifyCanExecuteChanged();
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
    private string _title = string.Empty;
    private string _titleEdit = string.Empty;
    private bool _isEditingTitle;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string TitleEdit
    {
        get => _titleEdit;
        set => SetProperty(ref _titleEdit, value);
    }

    public bool IsEditingTitle
    {
        get => _isEditingTitle;
        set => SetProperty(ref _isEditingTitle, value);
    }

    public Control? SettingsView { get; init; }

    public bool HasSettingsView => SettingsView != null;

    private bool _isSettingsOpen;
    public bool IsSettingsOpen
    {
        get => _isSettingsOpen;
        set => SetProperty(ref _isSettingsOpen, value);
    }
}
