using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ReCoPa.xAPI.ViewModels;

public partial class XApiStatementItemViewModel : ObservableObject
{
    public string Summary { get; }
    public string Actor { get; }
    public string Verb { get; }
    public string Object { get; }
    public DateTimeOffset Timestamp { get; }
    public string RawJson { get; }

    public string TimestampText => Timestamp.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss");
    public string TimeAgo => ToTimeAgo(Timestamp);

    // VM exposed action (View sets this)
    public Action<string>? CopyToClipboard { get; set; }

    public ICommand CopyJsonCommand { get; }

    public XApiStatementItemViewModel(string summary, string actor, string verb, string obj, DateTimeOffset ts, string rawJson)
    {
        Summary = summary;
        Actor = actor;
        Verb = verb;
        Object = obj;
        Timestamp = ts;
        RawJson = rawJson;

        CopyJsonCommand = new RelayCommand(() => CopyToClipboard?.Invoke(RawJson));
    }

    private static string ToTimeAgo(DateTimeOffset ts)
    {
        var delta = DateTimeOffset.Now - ts;
        if (delta.TotalSeconds < 60) return $"{Math.Max(1, (int)delta.TotalSeconds)} seconds ago";
        if (delta.TotalMinutes < 60) return $"{(int)delta.TotalMinutes} minutes ago";
        if (delta.TotalHours < 24) return $"{(int)delta.TotalHours} hours ago";
        return $"{(int)delta.TotalDays} days ago";
    }
}