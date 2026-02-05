using System;

namespace ReCoPa.Plugins;

public abstract class PluginPackageBase : IPluginPackage
{
    private PluginStorage? _storage;

    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract Contributor[] Contributors { get; }
    public abstract string Description { get; }
    public abstract IPluginComponent[] Components { get; }
    public abstract string Website { get; }
    public abstract string Repository { get; }
    public abstract string ChangelogUrl { get; }

    protected virtual string StorageFolderName => Name;

    protected PluginStorage Storage => _storage ??= new PluginStorage(
        PluginPaths.GetPluginDataDirectory(StorageFolderName));

    public virtual T LoadSettings<T>(string fileName, T fallback) where T : class
        => Storage.TryLoad(fileName, out T? data) && data is not null ? data : fallback;

    public virtual void SaveSettings<T>(string fileName, T data) where T : class
        => Storage.Save(fileName, data);

    public virtual T LoadVisualizationSettings<T>(string visualizationName, T fallback) where T : class
        => Storage.TryLoadVisualization(visualizationName, out T? data) && data is not null ? data : fallback;

    public virtual void SaveVisualizationSettings<T>(string visualizationName, T data) where T : class
        => Storage.SaveVisualization(visualizationName, data);
}
