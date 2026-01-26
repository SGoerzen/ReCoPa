using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ReCoPa.ViewModels;

public partial class VisualizationsViewModel : ObservableObject
{
    [ObservableProperty] private string liveSummary = "Waiting for live metrics…";
    [ObservableProperty] private string statementRateText = "0 statements/sec";

    public ObservableCollection<VisualizationItemViewModel> LatestStatements { get; } = new();

    public VisualizationsViewModel()
    {
        // Demo content (damit die View nicht leer ist)
        LatestStatements.Add(new VisualizationItemViewModel
        {
            Title = "experienced • https://example.org/activity/intro",
            Subtitle = "Actor: sergej@example.org • 2s ago"
        });

        LatestStatements.Add(new VisualizationItemViewModel
        {
            Title = "answered • https://example.org/activity/quiz/1",
            Subtitle = "Score: 8/10 • 8s ago"
        });
    }
}

public class VisualizationItemViewModel
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
}