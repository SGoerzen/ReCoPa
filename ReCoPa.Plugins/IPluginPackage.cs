namespace ReCoPa.Plugins;

public interface IPluginPackage
{
    string Id { get; }          
    string Name { get; }
    Contributor[] Contributors { get; }
    string Description { get; }
    IPluginComponent[] Components { get; }
    string Website { get; }
    string Repository { get; }
    string ChangelogUrl { get; }
    public string GetVersion() => GetType().Assembly.GetName().Version?.ToString() ?? "0.0.0";
    public string GetFilePath() => GetType().Assembly.Location;
}