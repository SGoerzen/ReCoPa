using System;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ReCoPa.Plugins;
using ReCoPa.XApi;
using ReCoPa.xAPI;

namespace ReCoPa.xAPI.ViewModels;

public partial class FocusItem : ObservableObject
{
    [ObservableProperty] private string objectId = string.Empty;
    [ObservableProperty] private int score;
}

public class FocusDistributionViewModel
{
    private const string DefaultStatementEvent = "statements";

    public ObservableCollection<FocusItem> Items { get; } = new();

    public void OnXApiStatement(XApiStatement stmt)
    {
        if (!stmt.IsGaze) return;

        var item = Items.FirstOrDefault(i => i.ObjectId == stmt.ObjectId);
        if (item == null)
        {
            item = new FocusItem { ObjectId = stmt.ObjectId };
            Items.Add(item);
        }

        item.Score = Math.Min(100, item.Score + 10);
    }

    public void SetDataAccess(IDataAccess access)
    {
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
