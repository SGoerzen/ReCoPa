using Avalonia.Controls;
using System.Collections.ObjectModel;
using ReCoPa.ViewModels;

namespace ReCoPa.Views;

public partial class VisualizationContainerView : UserControl
{
    public VisualizationContainerView()
    {
        InitializeComponent();
        DataContext = new VisualizationContainerViewModel();
    }
}