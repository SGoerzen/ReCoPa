using System;

namespace ReCoPa.Models;

[Serializable]
public struct TrackingMeta
{
    public bool isTracking;
    public bool isTrackingPaused;
    public bool isCalibrated;
    public bool isCalibratable;
    public string computerName;
    public string actorName;
    public string actorEmail;
    public string metaContext;
    public string sessionId;
        
    public static readonly TrackingMeta Empty = new TrackingMeta();
}
