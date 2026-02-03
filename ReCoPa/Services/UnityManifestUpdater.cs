using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ReCoPa.Services;

public static class UnityManifestUpdater
{
    public static async Task<string> UpdateAsync(string manifestPath)
    {
        try
        {
            if (!File.Exists(manifestPath))
                return "manifest.json not found.";

            var text = await File.ReadAllTextAsync(manifestPath);
            if (JsonNode.Parse(text) is not JsonObject root)
                return "Invalid manifest.json.";

            var scopedRegistries = root["scopedRegistries"] as JsonArray;
            if (scopedRegistries == null)
            {
                scopedRegistries = new JsonArray();
                root["scopedRegistries"] = scopedRegistries;
            }

            var npmEntry = FindScopedRegistry(scopedRegistries, "npm.js", "https://registry.npmjs.com");
            if (npmEntry == null)
            {
                npmEntry = new JsonObject
                {
                    ["name"] = "npm.js",
                    ["url"] = "https://registry.npmjs.com",
                    ["scopes"] = new JsonArray("com.rwth.unity.omilaxr")
                };
                scopedRegistries.Add(npmEntry);
            }
            else
            {
                var scopes = npmEntry["scopes"] as JsonArray;
                if (scopes == null)
                {
                    scopes = new JsonArray();
                    npmEntry["scopes"] = scopes;
                }

                if (!HasScope(scopes, "com.rwth.unity.omilaxr"))
                    scopes.Add("com.rwth.unity.omilaxr");
            }

            var dependencies = root["dependencies"] as JsonObject;
            if (dependencies == null)
            {
                dependencies = new JsonObject();
                root["dependencies"] = dependencies;
            }

            dependencies["com.rwth.unity.omilaxr.recopa"] = "0.0.1";

            var options = new JsonSerializerOptions { WriteIndented = true };
            await File.WriteAllTextAsync(manifestPath, root.ToJsonString(options));

            return "Updated manifest.json for com.rwth.unity.omilaxr.recopa.";
        }
        catch (Exception ex)
        {
            return $"Failed to update manifest.json: {ex.Message}";
        }
    }

    private static JsonObject? FindScopedRegistry(JsonArray scopedRegistries, string name, string url)
    {
        foreach (var node in scopedRegistries)
        {
            if (node is not JsonObject obj) continue;

            var existingName = obj["name"]?.GetValue<string>();
            var existingUrl = obj["url"]?.GetValue<string>();

            if (string.Equals(existingName, name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(existingUrl, url, StringComparison.OrdinalIgnoreCase))
            {
                return obj;
            }
        }

        return null;
    }

    private static bool HasScope(JsonArray scopes, string scope)
    {
        foreach (var node in scopes)
        {
            if (node?.GetValue<string>() == scope)
                return true;
        }

        return false;
    }
}
