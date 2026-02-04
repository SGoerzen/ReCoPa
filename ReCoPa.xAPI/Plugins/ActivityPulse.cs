using ReCoPa.Plugins;
using ReCoPa.xAPI.Views;

namespace ReCoPa.xAPI.Plugins;

public sealed class ActivityPulse : IVisualization
{
    public string Name => "Activity Pulse";
    public object CreateView()
    {
        return new ActivityPulseView();
    }

    public void ConsumeData(object data)
    {
        throw new NotImplementedException();
    }
}