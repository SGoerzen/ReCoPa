namespace ReCoPa.Plugins;

public interface IPluginPackage
{
    string Id { get; }          
    string Name { get; }
    Contributor[] Contributors { get; }
    string Description { get; }
    IPluginComponent[] Components { get; }

    public string GetVersion() => GetType().Assembly.GetName().Version?.ToString() ?? "0.0.0";
    public string GetFilePath() => GetType().Assembly.Location;
}