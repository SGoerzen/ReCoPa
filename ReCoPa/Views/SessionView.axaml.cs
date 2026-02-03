using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ReCoPa.Views;

public partial class SessionView : UserControl
{
    public SessionView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
