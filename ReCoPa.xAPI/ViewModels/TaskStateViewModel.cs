using System;
using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ReCoPa.Plugins;
using ReCoPa.XApi;
using ReCoPa.xAPI;

namespace ReCoPa.xAPI.ViewModels;



public partial class TaskStateItem : ObservableObject
{
    [ObservableProperty] private string taskId = string.Empty;
    [ObservableProperty] private string state = string.Empty;

    public IBrush StateColor =>
        State switch
        {
            "completed" => Brushes.LightGreen,
            "failed" => Brushes.IndianRed,
            "in-progress" => Brushes.LightBlue,
            _ => Brushes.LightGray
        };

    partial void OnStateChanged(string value)
    {
        OnPropertyChanged(nameof(StateColor));
    }
}

public class TaskStateViewModel
{
    private const string DefaultStatementEvent = "statements";

    public ObservableCollection<TaskStateItem> Tasks { get; } = new();

    public void OnXApiStatement(XApiStatement stmt)
    {
        if (!stmt.IsTaskRelated) return;

        var task = Tasks.FirstOrDefault(t => t.TaskId == stmt.ObjectId)
                   ?? new TaskStateItem { TaskId = stmt.ObjectId };

        task.State = stmt.Verb;
        if (!Tasks.Contains(task))
            Tasks.Add(task);
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
