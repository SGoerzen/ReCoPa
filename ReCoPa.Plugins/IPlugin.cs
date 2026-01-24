namespace ReCoPa.Plugins;

public interface IPlugin
{
    string Id { get; }          
    string Name { get; }
    Contributor[] Contributors { get; }
    string Description { get; }
}