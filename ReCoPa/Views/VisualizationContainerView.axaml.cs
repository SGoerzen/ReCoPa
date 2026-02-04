using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using ReCoPa.ViewModels;

namespace ReCoPa.Views;

public partial class VisualizationContainerView : UserControl
{
    public VisualizationContainerView()
    {
        InitializeComponent();
        if (Design.IsDesignMode)
            DataContext = new VisualizationContainerViewModel();
    }
}
