namespace ReCoPa.Plugins;

public interface IPlugin
{
    string Id { get; }          
    string Name { get; }
    string Version { get; }
    Contributor[] Contributors { get; }
    string Description { get; }
}