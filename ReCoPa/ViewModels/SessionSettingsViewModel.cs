using System.Collections.ObjectModel;
using System.Linq;
using Bogus;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReCoPa.Services;

namespace ReCoPa.ViewModels;

public partial class SessionSettingsViewModel : ObservableObject
{
    [ObservableProperty] private string actorName = "Anonymous";
    [ObservableProperty] private string actorMeta = "Anonymous | Lab PC";
    [ObservableProperty] private string actorEmail = "anonymous@omilaxr.dev";
    [ObservableProperty] private string actorNameEdit = string.Empty;
    [ObservableProperty] private string actorEmailEdit = string.Empty;
    [ObservableProperty] private bool isEditingActor;

    public ObservableCollection<EndpointSummaryItem> Endpoints { get; } = new();
    public ObservableCollection<SelectionItem> FilterOptions { get; } = new();
    public ObservableCollection<SelectionItem> ActionOptions { get; } = new();
    public ObservableCollection<SelectionItem> GestureOptions { get; } = new();

    public ObservableCollection<string> FiltersPreview { get; } = new();
    public ObservableCollection<string> ActionsPreview { get; } = new();
    public ObservableCollection<string> GesturesPreview { get; } = new();

    [ObservableProperty] private bool filtersHasMore;
    [ObservableProperty] private bool actionsHasMore;
    [ObservableProperty] private bool gesturesHasMore;

    public SessionSettingsViewModel()
    {
        Endpoints.Add(new EndpointSummaryItem("Local JSON File", true));
        Endpoints.Add(new EndpointSummaryItem("Learning Record Store", true));

        FilterOptions.Add(new SelectionItem("Exclude GameObjects", true));
        FilterOptions.Add(new SelectionItem("Exclude Gestures", true));
        FilterOptions.Add(new SelectionItem("Exclude Components", false));
        FilterOptions.Add(new SelectionItem("Normalize Collisions", false));
        FilterOptions.Add(new SelectionItem("Ignore Stationary Objects", false));
        FilterOptions.Add(new SelectionItem("Remove Outliers", true));

        ActionOptions.Add(new SelectionItem("Exclude actions", true));
        ActionOptions.Add(new SelectionItem("Delay actions", true));
        ActionOptions.Add(new SelectionItem("Normalize verbs", false));
        ActionOptions.Add(new SelectionItem("Collapse duplicates", false));
        ActionOptions.Add(new SelectionItem("Limit burst actions", false));
        ActionOptions.Add(new SelectionItem("Remove idle actions", false));

        GestureOptions.Add(new SelectionItem("Exclude gestures", true));
        GestureOptions.Add(new SelectionItem("Normalize gestures", true));
        GestureOptions.Add(new SelectionItem("Deduplicate gestures", false));
        GestureOptions.Add(new SelectionItem("Filter low confidence", false));
        GestureOptions.Add(new SelectionItem("Merge similar", false));
        GestureOptions.Add(new SelectionItem("Sort by intensity", false));

        ApplyPreview(FilterOptions, FiltersPreview, out var filtersMore);
        ApplyPreview(ActionOptions, ActionsPreview, out var actionsMore);
        ApplyPreview(GestureOptions, GesturesPreview, out var gesturesMore);
        FiltersHasMore = filtersMore;
        ActionsHasMore = actionsMore;
        GesturesHasMore = gesturesMore;
    }

    [RelayCommand]
    private void StartEditActor()
    {
        ActorNameEdit = ActorName;
        ActorEmailEdit = ActorEmail;
        IsEditingActor = true;
    }

    [RelayCommand]
    private void SaveActorEdit()
    {
        var name = ActorNameEdit?.Trim();
        var email = ActorEmailEdit?.Trim();

        if (!string.IsNullOrWhiteSpace(name))
            ActorName = name;
        if (!string.IsNullOrWhiteSpace(email))
            ActorEmail = email;

        IsEditingActor = false;
    }

    [RelayCommand]
    private void CancelActorEdit()
    {
        ActorNameEdit = ActorName;
        ActorEmailEdit = ActorEmail;
        IsEditingActor = false;
    }

    [RelayCommand]
    private void ConfigureEndpoints()
    {
        SukiDialogService.ShowEndpointsDialog(Endpoints);
    }

    [RelayCommand]
    private void GenerateActorPseudonym()
    {
        var faker = new Faker("en");
        var adjective = ToTitleCase(PickShortWord(faker, isAdjective: true, maxLen: 6));
        var noun = ToTitleCase(PickShortWord(faker, isAdjective: false, maxLen: 7));
        var pseudonym = $"{adjective}{noun}";

        ActorName = pseudonym;
        if (IsEditingActor)
            ActorNameEdit = pseudonym;
    }

    [RelayCommand]
    private void StartEditFilters()
    {
        SyncTempSelections(FilterOptions);
    }

    [RelayCommand]
    private void SaveFilters()
    {
        CommitTempSelections(FilterOptions);
        ApplyPreview(FilterOptions, FiltersPreview, out var hasMore);
        FiltersHasMore = hasMore;
    }

    [RelayCommand]
    private void CancelFilters()
    {
        RevertTempSelections(FilterOptions);
    }

    [RelayCommand]
    private void StartEditActions()
    {
        SyncTempSelections(ActionOptions);
    }

    [RelayCommand]
    private void SaveActions()
    {
        CommitTempSelections(ActionOptions);
        ApplyPreview(ActionOptions, ActionsPreview, out var hasMore);
        ActionsHasMore = hasMore;
    }

    [RelayCommand]
    private void CancelActions()
    {
        RevertTempSelections(ActionOptions);
    }

    [RelayCommand]
    private void StartEditGestures()
    {
        SyncTempSelections(GestureOptions);
    }

    [RelayCommand]
    private void SaveGestures()
    {
        CommitTempSelections(GestureOptions);
        ApplyPreview(GestureOptions, GesturesPreview, out var hasMore);
        GesturesHasMore = hasMore;
    }

    [RelayCommand]
    private void CancelGestures()
    {
        RevertTempSelections(GestureOptions);
    }

    private static void SyncTempSelections(ObservableCollection<SelectionItem> items)
    {
        foreach (var item in items)
            item.IsTempSelected = item.IsSelected;
    }

    private static void CommitTempSelections(ObservableCollection<SelectionItem> items)
    {
        foreach (var item in items)
            item.IsSelected = item.IsTempSelected;
    }

    private static void RevertTempSelections(ObservableCollection<SelectionItem> items)
    {
        foreach (var item in items)
            item.IsTempSelected = item.IsSelected;
    }

    private static void ApplyPreview(ObservableCollection<SelectionItem> items,
        ObservableCollection<string> preview,
        out bool hasMore)
    {
        preview.Clear();
        var selected = items.Where(i => i.IsSelected).Select(i => i.Label).ToList();
        foreach (var label in selected.Take(5))
            preview.Add(label);
        hasMore = selected.Count > 5;
    }

    private static string ToTitleCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var chars = value.Trim().ToLowerInvariant().ToCharArray();
        chars[0] = char.ToUpperInvariant(chars[0]);
        return new string(chars);
    }

    private static string PickShortWord(Faker faker, bool isAdjective, int maxLen)
    {
        var fallback = isAdjective
            ? new[] { "Naked", "Quick", "Bright", "Calm", "Swift", "Happy", "Sharp", "Quiet" }
            : new[] { "Tomato", "Fox", "Lemon", "River", "Panda", "Leaf", "Cloud", "Tiger" };

        for (var i = 0; i < 12; i++)
        {
            var word = isAdjective
                ? faker.Random.ListItem(new[]
                {
                    faker.Commerce.ProductAdjective(),
                    faker.Commerce.Color(),
                    faker.Random.Word()
                })
                : faker.Random.ListItem(new[]
                {
                    faker.Hacker.Noun(),
                    faker.Random.Word()
                });

            var cleaned = new string(word.Where(char.IsLetter).ToArray());
            if (!string.IsNullOrWhiteSpace(cleaned) && cleaned.Length <= maxLen)
                return cleaned;
        }

        return faker.Random.ListItem(fallback);
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

public partial class SelectionItem : ObservableObject
{
    [ObservableProperty] private string label;
    [ObservableProperty] private bool isSelected;
    [ObservableProperty] private bool isTempSelected;

    public SelectionItem(string label, bool isSelected)
    {
        this.label = label;
        this.isSelected = isSelected;
        isTempSelected = isSelected;
    }
}
