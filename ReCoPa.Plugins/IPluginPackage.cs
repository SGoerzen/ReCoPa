namespace ReCoPa.Plugins;

public interface IPluginPackage
{
    string Id { get; }          
    string Name { get; }
    Contributor[] Contributors { get; }
    string Description { get; }
    IPluginComponent[] Components { get; }
}