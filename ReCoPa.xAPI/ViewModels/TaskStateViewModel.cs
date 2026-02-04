using System.Collections.ObjectModel;
using Avalonia.Media;
using ReCoPa.XApi;

namespace ReCoPa.xAPI.ViewModels;



public class TaskStateItem
{
    public string TaskId { get; set; }
    public string State { get; set; }

    public IBrush StateColor =>
        State switch
        {
            "completed" => Brushes.LightGreen,
            "failed" => Brushes.IndianRed,
            "in-progress" => Brushes.LightBlue,
            _ => Brushes.LightGray
        };
}

public class TaskStateViewModel
{
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
}