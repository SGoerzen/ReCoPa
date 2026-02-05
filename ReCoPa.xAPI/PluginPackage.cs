using ReCoPa.Plugins;
using ReCoPa.xAPI.Plugins;

namespace ReCoPa.xAPI;

public class PluginPackage : PluginPackageBase
{
    public override string Id => "com.rwth.recopa.xapi";
    public override string Name => "ReCoPa.xAPI";

    public override Contributor[] Contributors =>
    [
        new Contributor
            { Name = "Sergej Görzen", Github = "https://github.com/SGoerzen", Email = "goerzen@cs.rwth-aachen.de" }
    ];
    public override string Description => "Plugin enabling xAPI and LRS.";
    public override IPluginComponent[] Components => [
        new ActivityPulse(),
        new FocusDistribution(),
        new TaskState(),
        new xApiPreview(),
        new LearningRecordStore()
    ];
    public override string Website => "https://omilaxr.dev/recopa";
    public override string Repository => "https://github.com/SGoerzen/ReCoPa.xAPI";
    public override string ChangelogUrl => "https://github.com/SGoerzen/ReCoPa.xAPI/blob/main/CHANGELOG.md";
}
