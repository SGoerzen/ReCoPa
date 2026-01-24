using ReCoPa.Plugins;

namespace ReCoPa.xAPI;

public class LearningRecordStore : IEndpoint
{
    public string Name => "Learning Record Store";
    public string EndpointReference => "OmiLAXR.xAPI.Endpoints.LearningRecordStore";
}