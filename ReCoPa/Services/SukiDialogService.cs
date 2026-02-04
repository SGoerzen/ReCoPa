using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using SukiUI.Dialogs;
using ReCoPa.Views;
using ReCoPa.ViewModels;

namespace ReCoPa.Services;

public static class SukiDialogService
{
    public static void ShowEndpointsDialog(IEnumerable<EndpointSummaryItem> endpoints)
    {
        var viewModel = new EndpointsDialogViewModel(endpoints);
        var view = new EndpointsDialogView
        {
            DataContext = viewModel
        };

        var builder = FluentSukiDialogBuilder.CreateDialog(MainWindow.DialogManager);
        builder.SetTitle("Data Endpoints");
        builder.SetContent(view);
        builder.SetCanDismissWithBackgroundClick(true);
        builder.AddActionButton("Close", _ => { }, true, []);

        MainWindow.DialogManager.TryShowDialog(builder.Dialog);
    }

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

    public static Task<bool> ConfirmVisualizationDeleteAsync(string title)
    {
        var tcs = new TaskCompletionSource<bool>();

        var content = new StackPanel
        {
            Spacing = 6,
            Children =
            {
                new TextBlock
                {
                    Text = $"Möchtest du \"{title}\" wirklich löschen?",
                    TextWrapping = TextWrapping.Wrap
                }
            }
        };

        var builder = FluentSukiDialogBuilder.CreateDialog(MainWindow.DialogManager);
        builder.SetTitle("Visualisierung löschen");
        builder.SetContent(content);
        builder.SetCanDismissWithBackgroundClick(true);
        builder.SetOnDismissed(_ => tcs.TrySetResult(false));
        builder.AddActionButton("Abbrechen", _ => tcs.TrySetResult(false), true, []);
        builder.AddActionButton("Löschen", _ => tcs.TrySetResult(true), true, []);

        if (!MainWindow.DialogManager.TryShowDialog(builder.Dialog))
            tcs.TrySetResult(false);

        return tcs.Task;
    }
}
