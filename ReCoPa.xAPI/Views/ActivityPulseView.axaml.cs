using Avalonia.Controls;
using ReCoPa.xAPI.ViewModels;

namespace ReCoPa.xAPI.Views;

public partial class ActivityPulseView : UserControl
{
    public ActivityPulseView()
        : this(new ActivityPulseViewModel())
    {
    }

    public ActivityPulseView(ActivityPulseViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
