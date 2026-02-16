using Avalonia.Controls;
using ReCoPa.xAPI.ViewModels;

namespace ReCoPa.xAPI.Views;

public partial class FocusDistributionView : UserControl
{
    public FocusDistributionView()
        : this(new FocusDistributionViewModel())
    {
    }

    public FocusDistributionView(FocusDistributionViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
