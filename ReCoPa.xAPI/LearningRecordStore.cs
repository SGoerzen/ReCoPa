using ReCoPa.Plugins;

namespace ReCoPa.xAPI;

public class LearningRecordStore : IEndpointPlugin
{
    public string Id => "ReCoPa.xAPI.LearningRecordStore";
    public string Name => "Learning Record Store";
    public string Version => "1.0.0";
    public Contributor[] Contributors =>
    [
        new() { Name = "Sergej Görzen", Github = "https://github.com/SGoerzen", Email = "goerzen@cs.rwth-aachen.de" }
    ];
    public string Description => "Endpoint for enabling xAPI and LRS.";
}