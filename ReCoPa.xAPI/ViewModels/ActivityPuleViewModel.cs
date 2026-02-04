using ReCoPa.XApi;

namespace ReCoPa.xAPI.ViewModels;

public class ActivityPulseViewModel
{
    public int ActivityLevel { get; private set; } // 0–100
    public string Description { get; private set; }

    public void OnXApiStatement(XApiStatement stmt)
    {
        ActivityLevel = Math.Min(100, ActivityLevel + 5);
        Description = $"Last event: {stmt.Verb}";
    }
}