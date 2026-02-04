using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ReCoPa.ViewModels;

public partial class SessionSettingsViewModel : ObservableObject
{
    [ObservableProperty] private string actorName = "Teach-PC2";
    [ObservableProperty] private string actorMeta = "Actor Name 1 | Lab PC";
    [ObservableProperty] private string actorEmail = "actor-mail@omilaxr.dev";

    public ObservableCollection<FilterSummaryItem> Filters { get; } = new();
    public ObservableCollection<EndpointSummaryItem> Endpoints { get; } = new();
    public ObservableCollection<ActionSummaryItem> Actions { get; } = new();
    public ObservableCollection<GestureSummaryItem> Gestures { get; } = new();

    public SessionSettingsViewModel()
    {
        Filters.Add(new FilterSummaryItem("1 GameObject excluded"));
        Filters.Add(new FilterSummaryItem("1 Gesture excluded"));
        Filters.Add(new FilterSummaryItem("1 ThirdOption excluded"));

        Endpoints.Add(new EndpointSummaryItem("Local JSON File", true));
        Endpoints.Add(new EndpointSummaryItem("Learning Record Store", true));

        Actions.Add(new ActionSummaryItem("2 actions excluded"));
        Actions.Add(new ActionSummaryItem("1 action delayed"));

        Gestures.Add(new GestureSummaryItem("1 gesture excluded"));
        Gestures.Add(new GestureSummaryItem("2 gestures normalized"));
    }
}

public partial class FilterSummaryItem : ObservableObject
{
    [ObservableProperty] private string label;

    public FilterSummaryItem(string label)
    {
        this.label = label;
    }
}

public partial class EndpointSummaryItem : ObservableObject
{
    [ObservableProperty] private string label;
    [ObservableProperty] private bool isActive;

    public EndpointSummaryItem(string label, bool isActive)
    {
        this.label = label;
        this.isActive = isActive;
    }
}

public partial class ActionSummaryItem : ObservableObject
{
    [ObservableProperty] private string label;

    public ActionSummaryItem(string label)
    {
        this.label = label;
    }
}

public partial class GestureSummaryItem : ObservableObject
{
    [ObservableProperty] private string label;

    public GestureSummaryItem(string label)
    {
        this.label = label;
    }
}
