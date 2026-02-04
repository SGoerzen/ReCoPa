using ReCoPa.Plugins;
using ReCoPa.xAPI.Views;

namespace ReCoPa.xAPI.Plugins;

public sealed class TaskState : IVisualization
{
    public string Name => "Task State";
    public object CreateView()
    {
        return new XApiPreviewView();
    }

    public void ConsumeData(object data)
    {
        throw new NotImplementedException();
    }
}