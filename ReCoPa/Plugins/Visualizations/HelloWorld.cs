using ReCoPa.Views.Visualizations;

namespace ReCoPa.Plugins.Visualizations;

public class HelloWorld : IVisualization
{
    public string Name => "Hello World";
    public object CreateView()
        => new HelloWorldView();
}