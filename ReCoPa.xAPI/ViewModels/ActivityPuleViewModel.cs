using System;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ReCoPa.Plugins;
using ReCoPa.XApi;
using ReCoPa.xAPI;

namespace ReCoPa.xAPI.ViewModels;

public partial class ActivityPulseViewModel : ObservableObject
{
    private const string DefaultStatementEvent = "statements";

    [ObservableProperty] private int activityLevel; // 0–100
    [ObservableProperty] private string description = string.Empty;

    private IDataAccess? _access;

    public void OnXApiStatement(XApiStatement stmt)
    {
        ActivityLevel = Math.Min(100, ActivityLevel + 5);
        Description = $"Last event: {stmt.Verb}";
    }

    public void SetDataAccess(IDataAccess access)
    {
        _access = access;
        var items = access.Store.Query(new DataQuery
        {
            EventName = DefaultStatementEvent,
            Limit = 500,
            NewestFirst = false
        });

        foreach (var packet in items)
            OnData(packet);
    }

    public void OnData(DataPacket packet)
    {
        if (!string.Equals(packet.EventName, DefaultStatementEvent, StringComparison.Ordinal)
            && !packet.EventName.EndsWith(":statements", StringComparison.OrdinalIgnoreCase))
            return;

        if (!XApiStatementParser.TryParse(packet.Payload, packet.TimestampUtc, out var stmt))
            return;

        Dispatcher.UIThread.Post(() => OnXApiStatement(stmt));
    }
}
