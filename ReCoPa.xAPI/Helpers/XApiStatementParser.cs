using System;
using System.Text.Json;
using ReCoPa.XApi;

namespace ReCoPa.xAPI;

public static class XApiStatementParser
{
    public static bool TryParse(string payload, DateTime fallbackUtc, out XApiStatement statement)
    {
        statement = new XApiStatement();
        if (string.IsNullOrWhiteSpace(payload))
            return false;

        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = NormalizeRoot(doc.RootElement);

            var actor = ReadActor(root);
            var verb = ReadVerb(root);
            var obj = ReadObject(root);

            if (string.IsNullOrWhiteSpace(actor)
                && string.IsNullOrWhiteSpace(verb)
                && string.IsNullOrWhiteSpace(obj))
                return false;

            statement.Actor = actor ?? string.Empty;
            statement.Verb = verb ?? string.Empty;
            statement.ObjectId = obj ?? string.Empty;
            statement.Timestamp = ReadTimestamp(root, fallbackUtc);

            var activityType = ReadActivityType(root);
            statement.ActivityType = activityType ?? string.Empty;

            statement.IsGaze = IsGaze(root, verb, activityType);
            statement.IsInteraction = IsInteraction(verb);
            statement.IsTaskRelated = IsTaskRelated(verb, obj, activityType);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static JsonElement NormalizeRoot(JsonElement root)
    {
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

        return root;
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

    private static DateTime ReadTimestamp(JsonElement root, DateTime fallbackUtc)
    {
        var ts = GetString(root, "timestamp") ?? GetString(root, "stored") ?? GetString(root, "time");
        if (!string.IsNullOrWhiteSpace(ts) && DateTime.TryParse(ts, out var parsed))
            return parsed.ToUniversalTime();

        return fallbackUtc;
    }

    private static string? ReadActivityType(JsonElement root)
    {
        if (!root.TryGetProperty("object", out var obj) || obj.ValueKind != JsonValueKind.Object)
            return null;

        if (!obj.TryGetProperty("definition", out var def) || def.ValueKind != JsonValueKind.Object)
            return null;

        var type = GetString(def, "type");
        if (string.IsNullOrWhiteSpace(type))
            return null;

        var idx = type.LastIndexOf('/');
        return idx >= 0 && idx < type.Length - 1 ? type[(idx + 1)..] : type;
    }

    private static bool IsGaze(JsonElement root, string? verb, string? activityType)
    {
        if (string.Equals(verb, "experienced", StringComparison.OrdinalIgnoreCase))
            return true;

        if (string.Equals(activityType, "gaze", StringComparison.OrdinalIgnoreCase))
            return true;

        if (root.TryGetProperty("context", out var ctx) && ctx.ValueKind == JsonValueKind.Object
            && ctx.TryGetProperty("extensions", out var ext) && ext.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in ext.EnumerateObject())
            {
                if (prop.Name.Contains("gaze", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        return false;
    }

    private static bool IsInteraction(string? verb)
    {
        if (string.IsNullOrWhiteSpace(verb))
            return false;

        return verb.Contains("interact", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTaskRelated(string? verb, string? obj, string? activityType)
    {
        if (string.Equals(activityType, "task", StringComparison.OrdinalIgnoreCase))
            return true;

        if (!string.IsNullOrWhiteSpace(verb)
            && (verb.Equals("completed", StringComparison.OrdinalIgnoreCase)
                || verb.Equals("failed", StringComparison.OrdinalIgnoreCase)
                || verb.Equals("progressed", StringComparison.OrdinalIgnoreCase)))
            return true;

        return !string.IsNullOrWhiteSpace(obj)
            && obj.Contains("task", StringComparison.OrdinalIgnoreCase);
    }

    private static string? GetString(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value))
            return null;
        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }
}
