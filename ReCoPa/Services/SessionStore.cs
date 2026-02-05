using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ReCoPa.ViewModels;

namespace ReCoPa.Services;

public static class SessionStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string BasePath => Path.Combine(Path.GetTempPath(), "ReCoPa", "Sessions");

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

    public static IReadOnlyList<SessionSnapshot> LoadSessions()
    {
        if (!Directory.Exists(BasePath))
            return Array.Empty<SessionSnapshot>();

        var sessions = new List<SessionSnapshot>();
        foreach (var file in Directory.EnumerateFiles(BasePath, "session.json", SearchOption.AllDirectories))
        {
            try
            {
                var json = File.ReadAllText(file);
                var snapshot = JsonSerializer.Deserialize<SessionSnapshot>(json);
                if (snapshot != null)
                    sessions.Add(snapshot);
            }
            catch
            {
                // Ignore corrupted session data.
            }
        }

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
            CreatedUtc = DateTime.UtcNow,
            //Settings = session.Settings.ToSnapshot(),
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
}

public sealed class VisualizationSnapshot
{
    public string VisualizationName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string DataFile { get; set; } = string.Empty;
}
