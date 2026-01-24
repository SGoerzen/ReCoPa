namespace ReCoPa.Plugins;

public abstract class LocalFileEndpoint : IEndpointPlugin
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string Version { get; }
    public abstract Contributor[] Contributors { get; }
    public abstract string Description { get; }
}