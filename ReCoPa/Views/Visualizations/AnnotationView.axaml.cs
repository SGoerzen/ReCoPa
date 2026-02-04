using Avalonia.Controls;
using ReCoPa.ViewModels;

namespace ReCoPa.Views.Visualizations;

public partial class AnnotationView : UserControl
{
    public AnnotationView()
    {
        InitializeComponent();
        DataContext ??= new AnnotationViewModel();
    }
}
