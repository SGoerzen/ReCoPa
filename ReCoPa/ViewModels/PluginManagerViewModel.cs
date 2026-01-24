using System.Collections.ObjectModel;

namespace ReCoPa.ViewModels;

public class PluginManagerViewModel : ViewModelBase
{
    public ObservableCollection<PluginItemViewModel> Plugins { get; } = new();

    public PluginManagerViewModel()
    {
        App.PluginManager!.Load();
        var plugins = App.PluginManager.Plugins;
        foreach (var plugin in plugins)
            Plugins.Add(new PluginItemViewModel(plugin));
    }
}