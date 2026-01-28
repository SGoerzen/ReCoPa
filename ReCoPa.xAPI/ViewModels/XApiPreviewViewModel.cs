using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ReCoPa.xAPI.ViewModels;

public partial class XApiPreviewViewModel : ObservableObject
{
    public ObservableCollection<string> Filters { get; } = new()
    {
        "Filter: All Statements",
        "Filter: Only 'experienced'",
        "Filter: Only 'completed'"
    };

    [ObservableProperty] private string selectedFilter = "Filter: All Statements";

    public ObservableCollection<XApiStatementItemViewModel> Statements { get; } = new();

    public ICommand ExportCsvCommand { get; }

    public XApiPreviewViewModel()
    {
        ExportCsvCommand = new RelayCommand(() =>
        {
            // TODO: später implementieren
        });

        LoadMock();
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SelectedFilter))
                ApplyFilter();
        };
    }

    private ObservableCollection<XApiStatementItemViewModel> _all = new();

    private void LoadMock()
    {
        var now = DateTimeOffset.Now;

        _all = new ObservableCollection<XApiStatementItemViewModel>(new[]
        {
            new XApiStatementItemViewModel(
                summary: "Player X interacted with Object A",
                actor: "Player X (mailto:playerx@example.org)",
                verb: "interacted",
                obj: "https://example.org/object/A",
                ts: now.AddMinutes(-5),
                rawJson: """
                {
                  "actor": { "name": "Player X", "mbox": "mailto:playerx@example.org" },
                  "verb": { "id": "https://w3id.org/xapi/seriousgames/verbs/interacted", "display": {"en-US":"interacted"} },
                  "object": { "id": "https://example.org/object/A" },
                  "timestamp": "2026-01-20T13:22:00Z"
                }
                """
            ),
            new XApiStatementItemViewModel(
                summary: "Player X completed Task 1",
                actor: "Player X (mailto:playerx@example.org)",
                verb: "completed",
                obj: "https://example.org/tasks/1",
                ts: now.AddMinutes(-10),
                rawJson: """
                {
                  "actor": { "name": "Player X", "mbox": "mailto:playerx@example.org" },
                  "verb": { "id": "http://adlnet.gov/expapi/verbs/completed", "display": {"en-US":"completed"} },
                  "object": { "id": "https://example.org/tasks/1" },
                  "result": { "success": true, "completion": true, "duration": "PT32S" },
                  "timestamp": "2026-01-20T13:17:00Z"
                }
                """
            ),
            new XApiStatementItemViewModel(
                summary: "Player X looked at Target 1",
                actor: "Player X (mailto:playerx@example.org)",
                verb: "experienced",
                obj: "https://example.org/targets/1",
                ts: now.AddMinutes(-15),
                rawJson: """
                {
                  "actor": { "name": "Player X", "mbox": "mailto:playerx@example.org" },
                  "verb": { "id": "http://adlnet.gov/expapi/verbs/experienced", "display": {"en-US":"experienced"} },
                  "object": { "id": "https://example.org/targets/1" },
                  "context": {
                    "extensions": {
                      "https://recopa/extensions/gaze": { "durationMs": 1200, "confidence": 0.83 }
                    }
                  },
                  "timestamp": "2026-01-20T13:12:00Z"
                }
                """
            ),
            new XApiStatementItemViewModel(
                summary: "Player X reached Checkpoint 2",
                actor: "Player X (mailto:playerx@example.org)",
                verb: "progressed",
                obj: "https://example.org/checkpoints/2",
                ts: now.AddMinutes(-22),
                rawJson: """
                {
                  "actor": { "name": "Player X", "mbox": "mailto:playerx@example.org" },
                  "verb": { "id": "https://w3id.org/xapi/adl/verbs/progressed", "display": {"en-US":"progressed"} },
                  "object": { "id": "https://example.org/checkpoints/2" },
                  "context": { "platform": "Unity", "language": "en-US" },
                  "timestamp": "2026-01-20T13:05:00Z"
                }
                """
            ),
        });

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        Statements.Clear();

        var filtered = _all.AsEnumerable();

        if (SelectedFilter.Contains("experienced", StringComparison.OrdinalIgnoreCase))
            filtered = filtered.Where(s => string.Equals(s.Verb, "experienced", StringComparison.OrdinalIgnoreCase));

        if (SelectedFilter.Contains("completed", StringComparison.OrdinalIgnoreCase))
            filtered = filtered.Where(s => string.Equals(s.Verb, "completed", StringComparison.OrdinalIgnoreCase));

        foreach (var s in filtered)
            Statements.Add(s);
    }
}