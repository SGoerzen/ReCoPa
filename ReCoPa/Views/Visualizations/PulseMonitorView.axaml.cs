using Avalonia.Controls;
using ReCoPa.ViewModels.Visualizations;

namespace ReCoPa.Views.Visualizations;

public partial class PulseMonitorView : UserControl
{
    public PulseMonitorView()
    {
        InitializeComponent();
        DataContext ??= new PulseMonitorViewModel();
    }
}