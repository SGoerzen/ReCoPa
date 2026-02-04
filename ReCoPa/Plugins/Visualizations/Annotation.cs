using System;
using ReCoPa.Views.Visualizations;

namespace ReCoPa.Plugins.Visualizations;

public sealed class Annotation : IVisualization
{
    public string Name => "Annotation";
    public object CreateView()
    {
        return new AnnotationView();
    }

    public void ConsumeData(object data)
    {
        throw new NotImplementedException();
    }
}