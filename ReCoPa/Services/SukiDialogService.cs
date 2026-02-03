using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using SukiUI.Dialogs;
using ReCoPa.Views;

namespace ReCoPa.Services;

public static class SukiDialogService
{
    public static Task<bool> ConfirmSessionCloseAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        var content = new StackPanel
        {
            Spacing = 6,
            Children =
            {
                new TextBlock
                {
                    Text = "Möchtest du die Session wirklich schließen?",
                    TextWrapping = TextWrapping.Wrap
                },
                new TextBlock
                {
                    Text = "(Hinweis: sie kann später wiederhergestellt werden)",
                    TextWrapping = TextWrapping.Wrap,
                    Opacity = 0.7
                }
            }
        };

        var builder = FluentSukiDialogBuilder.CreateDialog(MainWindow.DialogManager);
        builder.SetTitle("Session schließen");
        builder.SetContent(content);
        builder.SetCanDismissWithBackgroundClick(true);
        builder.SetOnDismissed(_ => tcs.TrySetResult(false));
        builder.AddActionButton("Nein", _ => tcs.TrySetResult(false), true, []);
        builder.AddActionButton("Ja", _ => tcs.TrySetResult(true), true, []);

        if (!MainWindow.DialogManager.TryShowDialog(builder.Dialog))
            tcs.TrySetResult(false);

        return tcs.Task;
    }
}
