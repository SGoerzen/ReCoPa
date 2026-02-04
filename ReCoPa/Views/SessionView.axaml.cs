using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ReCoPa.Views;

public partial class SessionView : UserControl
{
    public SessionView()
    {
        InitializeComponent();
        AttachedToVisualTree += OnAttachedToVisualTree;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void OnAttachedToVisualTree(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        if (DataContext is not ViewModels.SessionViewModel vm)
            return;

        vm.Visualization.RefreshAvailableVisualizations();
    }
}
