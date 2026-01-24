using System.Collections.ObjectModel;
using Avalonia.Controls;

namespace ReCoPa.ViewModels;

public class VisualizationContainerViewModel
{
    public ObservableCollection<Control> Views { get; } = new();

    public VisualizationContainerViewModel()
    {
        foreach (var visualization in App.PluginManager.Visualizations)
        {
            try
            {
                var view = (Control)visualization.CreateView();
                Views.Add(view);
            }
            catch
            {
                // später: Logging / Error-View
            }
        }
    }
}