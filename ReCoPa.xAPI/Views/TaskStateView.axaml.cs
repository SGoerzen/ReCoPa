using Avalonia.Controls;
using ReCoPa.xAPI.ViewModels;

namespace ReCoPa.xAPI.Views;

public partial class TaskStateView : UserControl
{
    public TaskStateView()
        : this(new TaskStateViewModel())
    {
    }

    public TaskStateView(TaskStateViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
