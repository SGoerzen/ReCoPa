
namespace ReCoPa.Plugins;

public interface IVisualization : IPluginComponent
{
    object CreateView();
    void ConsumeData(object data);
}