using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using ReCoPa.ViewModels;

namespace ReCoPa.Views;

public partial class VisualizationContainerView : UserControl
{
    private const string DragFormat = "recopa/visualization-host-item";

    public VisualizationContainerView()
    {
        InitializeComponent();
        if (Design.IsDesignMode)
            DataContext = new VisualizationContainerViewModel();
    }

    private void OnTilePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control control)
            return;

        if (e.Source is Button)
            return;

        if (!e.GetCurrentPoint(control).Properties.IsLeftButtonPressed)
            return;

        if (control.DataContext is not VisualizationHostItem item)
            return;

        var data = new DataObject();
        data.Set(DragFormat, item);
        _ = DragDrop.DoDragDrop(e, data, DragDropEffects.Move);
    }

    private void OnTileDragOver(object? sender, DragEventArgs e)
    {
        if (!e.Data.Contains(DragFormat))
            return;

        if (sender is not Control control)
            return;

        var source = e.Data.Get(DragFormat) as VisualizationHostItem;
        var target = control.DataContext as VisualizationHostItem;

        if (source is null || target is null || ReferenceEquals(source, target))
        {
            e.DragEffects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        e.DragEffects = DragDropEffects.Move;
        e.Handled = true;
    }

    private void OnTileDrop(object? sender, DragEventArgs e)
    {
        if (!e.Data.Contains(DragFormat))
            return;

        if (sender is not Control control)
            return;

        if (DataContext is not VisualizationContainerViewModel vm)
            return;

        var source = e.Data.Get(DragFormat) as VisualizationHostItem;
        var target = control.DataContext as VisualizationHostItem;

        if (source is null || target is null || ReferenceEquals(source, target))
            return;

        var sourceIndex = vm.Views.IndexOf(source);
        var targetIndex = vm.Views.IndexOf(target);

        if (sourceIndex < 0 || targetIndex < 0 || sourceIndex == targetIndex)
            return;

        vm.Views.Move(sourceIndex, targetIndex);
        e.Handled = true;
    }
}
