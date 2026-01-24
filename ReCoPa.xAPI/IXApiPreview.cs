using ReCoPa.Plugins;
using ReCoPa.xAPI.Views;

namespace ReCoPa.xAPI;

public class IXApiPreview : IVisualization
{
    public string Name => "xAPI Preview";
    public object CreateView()
    {
        return new XApiPreviewView();
    }

    public void ConsumeData(object data)
    {
        throw new NotImplementedException();
    }
}