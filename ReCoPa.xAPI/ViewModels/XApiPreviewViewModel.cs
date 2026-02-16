using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReCoPa.Plugins;

namespace ReCoPa.xAPI.ViewModels;

public partial class XApiPreviewViewModel : ObservableObject
{
    private const int MaxItems = 2000;
    private const string DefaultStatementEvent = "statements";

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

        if (Design.IsDesignMode)
            LoadMock();
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SelectedFilter))
                ApplyFilter();
        };
    }

    private readonly List<XApiStatementItemViewModel> _all = new();
    private IDataAccess? _dataAccess;

    public void SetDataAccess(IDataAccess access)
    {
        _dataAccess = access;

        // Load recent statements from store (if any) for initial view.
        var items = access.Store.Query(new DataQuery
        {
            EventName = DefaultStatementEvent,
            Limit = MaxItems,
            NewestFirst = false
        });

        if (items.Count == 0)
            return;

        var parsed = new List<XApiStatementItemViewModel>();
        foreach (var packet in items)
        {
            if (TryCreateItem(packet, out var vm))
                parsed.Add(vm!);
        }

        if (parsed.Count == 0)
            return;

        Dispatcher.UIThread.Post(() =>
        {
            _all.Clear();
            _all.AddRange(parsed);
            ApplyFilter();
        });
    }

    public void OnData(DataPacket packet)
    {
        if (!string.Equals(packet.EventName, DefaultStatementEvent, StringComparison.Ordinal)
            && !packet.EventName.EndsWith(":statements", StringComparison.OrdinalIgnoreCase))
            return;

        if (!TryCreateItem(packet, out var item))
            return;

        Dispatcher.UIThread.Post(() => AddStatement(item!));
    }

    private void LoadMock()
    {
        var now = DateTimeOffset.Now;

        _all.Clear();
        _all.AddRange(new[]
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

    private void AddStatement(XApiStatementItemViewModel item)
    {
        _all.Add(item);

        if (_all.Count > MaxItems)
        {
            var overflow = _all.Count - MaxItems;
            for (var i = 0; i < overflow; i++)
            {
                var removed = _all[0];
                _all.RemoveAt(0);
                Statements.Remove(removed);
            }
        }

        if (MatchesFilter(item))
            Statements.Add(item);
    }

    private void ApplyFilter()
    {
        Statements.Clear();

        foreach (var s in _all.Where(MatchesFilter))
            Statements.Add(s);
    }

    private bool MatchesFilter(XApiStatementItemViewModel item)
    {
        if (SelectedFilter.Contains("experienced", StringComparison.OrdinalIgnoreCase))
            return string.Equals(item.Verb, "experienced", StringComparison.OrdinalIgnoreCase);

        if (SelectedFilter.Contains("completed", StringComparison.OrdinalIgnoreCase))
            return string.Equals(item.Verb, "completed", StringComparison.OrdinalIgnoreCase);

        return true;
    }

    private static bool TryCreateItem(DataPacket packet, out XApiStatementItemViewModel? item)
    {
        item = null;
        if (string.IsNullOrWhiteSpace(packet.Payload))
            return false;

        try
        {
            using var doc = JsonDocument.Parse(packet.Payload);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object)
                    root = data;
                if (root.TryGetProperty("statement", out var statement) && statement.ValueKind == JsonValueKind.Object)
                    root = statement;
                if (root.TryGetProperty("statements", out var statements) && statements.ValueKind == JsonValueKind.Array)
                {
                    var last = statements.EnumerateArray().LastOrDefault();
                    if (last.ValueKind == JsonValueKind.Object)
                        root = last;
                }
            }

            var actor = ReadActor(root);
            var verb = ReadVerb(root);
            var obj = ReadObject(root);
            var timestamp = ReadTimestamp(root, packet.TimestampUtc);

            if (string.IsNullOrWhiteSpace(actor) &&
                string.IsNullOrWhiteSpace(verb) &&
                string.IsNullOrWhiteSpace(obj))
                return false;

            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(actor)) parts.Add(actor);
            if (!string.IsNullOrWhiteSpace(verb)) parts.Add(verb);
            if (!string.IsNullOrWhiteSpace(obj)) parts.Add(obj);
            var summary = parts.Count == 0 ? "Statement" : string.Join(" ", parts);

            item = new XApiStatementItemViewModel(
                summary: summary,
                actor: actor ?? "",
                verb: verb ?? "",
                obj: obj ?? "",
                ts: timestamp,
                rawJson: packet.Payload);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string? ReadActor(JsonElement root)
    {
        if (!root.TryGetProperty("actor", out var actor) || actor.ValueKind != JsonValueKind.Object)
            return null;

        var name = GetString(actor, "name");
        var mbox = GetString(actor, "mbox");

        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(mbox))
            return $"{name} ({mbox})";
        if (!string.IsNullOrWhiteSpace(name))
            return name;
        if (!string.IsNullOrWhiteSpace(mbox))
            return mbox;

        return null;
    }

    private static string? ReadVerb(JsonElement root)
    {
        if (!root.TryGetProperty("verb", out var verb) || verb.ValueKind != JsonValueKind.Object)
            return null;

        if (verb.TryGetProperty("display", out var display) && display.ValueKind == JsonValueKind.Object)
        {
            if (display.TryGetProperty("en-US", out var en) && en.ValueKind == JsonValueKind.String)
                return en.GetString();

            foreach (var prop in display.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.String)
                    return prop.Value.GetString();
            }
        }

        var id = GetString(verb, "id");
        if (string.IsNullOrWhiteSpace(id))
            return null;

        var idx = id.LastIndexOf('/');
        return idx >= 0 && idx < id.Length - 1 ? id[(idx + 1)..] : id;
    }

    private static string? ReadObject(JsonElement root)
    {
        if (!root.TryGetProperty("object", out var obj) || obj.ValueKind != JsonValueKind.Object)
            return null;

        var id = GetString(obj, "id");
        if (!string.IsNullOrWhiteSpace(id))
            return id;

        if (obj.TryGetProperty("definition", out var def) && def.ValueKind == JsonValueKind.Object)
        {
            if (def.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.Object)
            {
                if (name.TryGetProperty("en-US", out var en) && en.ValueKind == JsonValueKind.String)
                    return en.GetString();

                foreach (var prop in name.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.String)
                        return prop.Value.GetString();
                }
            }
        }

        return null;
    }

    private static DateTimeOffset ReadTimestamp(JsonElement root, DateTime fallbackUtc)
    {
        var ts = GetString(root, "timestamp") ?? GetString(root, "stored") ?? GetString(root, "time");
        if (!string.IsNullOrWhiteSpace(ts) && DateTimeOffset.TryParse(ts, out var parsed))
            return parsed;

        return new DateTimeOffset(fallbackUtc, TimeSpan.Zero);
    }

    private static string? GetString(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value))
            return null;
        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }
}
