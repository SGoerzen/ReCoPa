using ReCoPa.Plugins;
using ReCoPa.xAPI.Views;

namespace ReCoPa.xAPI;

public sealed class xApiPreview : IVisualization
{
    public string Name => "xAPI Statements";
    public object CreateView()
    {
        return new XApiPreviewView();
    }

    public void ConsumeData(object data)
    {
        throw new NotImplementedException();
    }
}