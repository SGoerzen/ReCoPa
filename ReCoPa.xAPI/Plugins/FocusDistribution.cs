using ReCoPa.Plugins;
using ReCoPa.xAPI.Views;

namespace ReCoPa.xAPI.Plugins;

public sealed class FocusDistribution : IVisualization
{
    public string Name => "Focus Distribution";
    public object CreateView()
    {
        return new FocusDistributionView();
    }

    public void ConsumeData(object data)
    {
        throw new NotImplementedException();
    }
}