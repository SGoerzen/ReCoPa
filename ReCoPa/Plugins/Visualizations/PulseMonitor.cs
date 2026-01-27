
using ReCoPa.Views.Visualizations;

namespace ReCoPa.Plugins.Visualizations;

public class PulseMonitor : IVisualization
{
    public string Name => "Pulse Monitor";
    public object CreateView()
        => new PulseMonitorView();
}