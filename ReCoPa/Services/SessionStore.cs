using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ReCoPa.Plugins;
using ReCoPa.ViewModels;

namespace ReCoPa.Services;

public static class SessionStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private const string IndexFileName = "sessions.json";

    public static string BasePath => Path.Combine(PluginPaths.GetAppDataDirectory(), "Sessions");

    public static void SaveSession(SessionViewModel session)
    {
        if (session.SessionId == Guid.Empty)
            session.SessionId = Guid.NewGuid();

        Directory.CreateDirectory(BasePath);
        var folder = EnsureSessionFolder(session.SessionId, session.ClientName);

        var snapshot = SessionSnapshot.FromSession(session);
        snapshot.UpdatedUtc = DateTime.UtcNow;

        var metaPath = Path.Combine(folder, "session.json");
        File.WriteAllText(metaPath, JsonSerializer.Serialize(snapshot, JsonOptions));

        SaveVisualizationFiles(snapshot, folder);
    }

    public static void SaveSessions(IEnumerable<TabViewModel> tabs)
    {
        if (tabs == null)
            return;

        Directory.CreateDirectory(BasePath);
        var index = LoadIndex() ?? new SessionIndex();

        var snapshots = new List<SessionSnapshot>();
        foreach (var tab in tabs)
        {
            if (tab.Session == null)
                continue;

            if (tab.Session.SessionId == Guid.Empty)
                tab.Session.SessionId = Guid.NewGuid();

            var snapshot = SessionSnapshot.FromSession(tab.Session);
            snapshot.IsActive = tab.IsActive;
            snapshot.UpdatedUtc = DateTime.UtcNow;

            var folder = EnsureSessionFolder(snapshot.Id, snapshot.Name);
            var metaPath = Path.Combine(folder, "session.json");
            File.WriteAllText(metaPath, JsonSerializer.Serialize(snapshot, JsonOptions));
            SaveVisualizationFiles(snapshot, folder);

            snapshots.Add(snapshot);
        }

        index.SessionIds = snapshots.Select(s => s.Id).ToList();

        var indexPath = Path.Combine(BasePath, IndexFileName);
        File.WriteAllText(indexPath, JsonSerializer.Serialize(index, JsonOptions));
    }

    public static void MarkSessionClosed(Guid sessionId)
    {
        if (sessionId == Guid.Empty)
            return;

        Directory.CreateDirectory(BasePath);
        var index = LoadIndex() ?? new SessionIndex();
        if (!index.ClosedSessionIds.Contains(sessionId))
            index.ClosedSessionIds.Add(sessionId);
        index.SessionIds.Remove(sessionId);

        var indexPath = Path.Combine(BasePath, IndexFileName);
        File.WriteAllText(indexPath, JsonSerializer.Serialize(index, JsonOptions));
    }

    public static void MarkSessionReopened(Guid sessionId)
    {
        if (sessionId == Guid.Empty)
            return;

        Directory.CreateDirectory(BasePath);
        var index = LoadIndex() ?? new SessionIndex();
        index.ClosedSessionIds.Remove(sessionId);

        var indexPath = Path.Combine(BasePath, IndexFileName);
        File.WriteAllText(indexPath, JsonSerializer.Serialize(index, JsonOptions));
    }

    public static IReadOnlyList<SessionSnapshot> LoadSessions()
    {
        if (!Directory.Exists(BasePath))
            return Array.Empty<SessionSnapshot>();

        var index = LoadIndex();
        var orderedIds = index?.SessionIds;
        var closedIds = index?.ClosedSessionIds ?? new List<Guid>();

        var sessions = new List<SessionSnapshot>();
        IEnumerable<string> files = Directory.EnumerateFiles(BasePath, "session.json", SearchOption.AllDirectories);
        if (orderedIds is { Count: > 0 })
        {
            files = orderedIds
                .Select(id => Directory.GetDirectories(BasePath, $"{id}_*").FirstOrDefault())
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(d => Path.Combine(d!, "session.json"))
                .Where(File.Exists);
        }

        foreach (var file in files)
        {
            try
            {
                var json = File.ReadAllText(file);
                var snapshot = JsonSerializer.Deserialize<SessionSnapshot>(json);
                if (snapshot != null && !closedIds.Contains(snapshot.Id))
                    sessions.Add(snapshot);
            }
            catch
            {
                // Ignore corrupted session data.
            }
        }

        if (orderedIds is { Count: > 0 })
            return sessions;

        return sessions
            .OrderBy(s => s.UpdatedUtc == default ? s.CreatedUtc : s.UpdatedUtc)
            .ToList();
    }

    private static void SaveVisualizationFiles(SessionSnapshot snapshot, string folder)
    {
        foreach (var vis in snapshot.Visualizations)
        {
            if (string.IsNullOrWhiteSpace(vis.DataFile))
                continue;

            var path = Path.Combine(folder, vis.DataFile);
            if (File.Exists(path))
                continue;

            var payload = new
            {
                vis.VisualizationName,
                vis.Title
            };

            File.WriteAllText(path, JsonSerializer.Serialize(payload, JsonOptions));
        }
    }

    private static SessionIndex? LoadIndex()
    {
        try
        {
            var path = Path.Combine(BasePath, IndexFileName);
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<SessionIndex>(json);
        }
        catch
        {
            return null;
        }
    }

    private static string EnsureSessionFolder(Guid id, string? name)
    {
        var safeName = SanitizeFolderName(string.IsNullOrWhiteSpace(name) ? "Session" : name);
        var desired = Path.Combine(BasePath, $"{id}_{safeName}");

        var existing = Directory.GetDirectories(BasePath, $"{id}_*").FirstOrDefault();
        if (existing != null && !string.Equals(existing, desired, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                if (Directory.Exists(desired))
                    Directory.Delete(desired, true);
                Directory.Move(existing, desired);
            }
            catch
            {
                return existing;
            }
        }

        Directory.CreateDirectory(desired);
        return desired;
    }

    private static string SanitizeFolderName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Select(c => invalid.Contains(c) ? '-' : c).ToArray());
        cleaned = cleaned.Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? "Session" : cleaned;
    }
}

public sealed class SessionSnapshot
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "Session";
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedUtc { get; set; }
    public bool IsActive { get; set; }
    public string CurrentView { get; set; } = "Visualizations";
    public bool IsEyeTrackingEnabled { get; set; } = true;
    public bool IsTrackingRunning { get; set; }
    public bool IsTrackingPaused { get; set; }
    public int StatementsCount { get; set; }
    public int GameObjectsCount { get; set; }
    public double Fps { get; set; }
    public int HeartRate { get; set; }
    public double ScoreProgressValue { get; set; }
    public double ElapsedSeconds { get; set; }
    public int GridRows { get; set; } = 2;
    public int GridColumns { get; set; } = 2;
    public SessionSettingsSnapshot Settings { get; set; } = new();
    public List<VisualizationSnapshot> Visualizations { get; set; } = new();

    public static SessionSnapshot FromSession(SessionViewModel session)
    {
        var visSnapshots = new List<VisualizationSnapshot>();
        for (var i = 0; i < session.Visualization.Views.Count; i++)
        {
            var item = session.Visualization.Views[i];
            var dataFile = item.DataFileName;
            if (string.IsNullOrWhiteSpace(dataFile))
            {
                var suffix = SanitizeFilePart(item.Title);
                dataFile = $"vis_{i + 1}_{suffix}.json";
                item.DataFileName = dataFile;
            }

            visSnapshots.Add(new VisualizationSnapshot
            {
                VisualizationName = item.VisualizationName,
                Title = item.Title,
                DataFile = dataFile
            });
        }

        return new SessionSnapshot
        {
            Id = session.SessionId,
            Name = session.ClientName ?? "Session",
            CreatedUtc = session.CreatedUtc == default ? DateTime.UtcNow : session.CreatedUtc,
            CurrentView = session.CurrentView,
            IsEyeTrackingEnabled = session.IsEyeTrackingEnabled,
            IsTrackingRunning = session.IsTrackingRunning,
            IsTrackingPaused = session.IsTrackingPaused,
            StatementsCount = session.StatementsCount,
            GameObjectsCount = session.GameObjectsCount,
            Fps = session.Fps,
            HeartRate = session.HeartRate,
            ScoreProgressValue = session.ScoreProgressValue,
            ElapsedSeconds = session.ElapsedTime.TotalSeconds,
            GridRows = session.Visualization.GridRows,
            GridColumns = session.Visualization.GridColumns,
            Settings = session.Settings.ToSnapshot(),
            Visualizations = visSnapshots
        };
    }

    private static string SanitizeFilePart(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "viz";

        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(value.Select(c => invalid.Contains(c) ? '-' : c).ToArray());
        cleaned = cleaned.Replace(' ', '-');
        return string.IsNullOrWhiteSpace(cleaned) ? "viz" : cleaned.ToLowerInvariant();
    }
}

public sealed class SessionSettingsSnapshot
{
    public string ActorName { get; set; } = string.Empty;
    public string ActorMeta { get; set; } = string.Empty;
    public string ActorEmail { get; set; } = string.Empty;
    public List<string> Filters { get; set; } = new();
    public List<string> Actions { get; set; } = new();
    public List<string> Gestures { get; set; } = new();
    public List<ToggleSnapshot> Endpoints { get; set; } = new();
}

public sealed class VisualizationSnapshot
{
    public string VisualizationName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string DataFile { get; set; } = string.Empty;
}

public sealed class ToggleSnapshot
{
    public string Label { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}

public sealed class SessionIndex
{
    public List<Guid> SessionIds { get; set; } = new();
    public List<Guid> ClosedSessionIds { get; set; } = new();
}
