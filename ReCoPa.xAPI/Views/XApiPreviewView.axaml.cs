using Avalonia.Controls;
using ReCoPa.xAPI.ViewModels;

namespace ReCoPa.xAPI.Views;

public partial class XApiPreviewView : UserControl
{
    public XApiPreviewView()
        : this(new XApiPreviewViewModel())
    {
    }

    public XApiPreviewView(XApiPreviewViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        // hook clipboard action for all items (and future items)
        void WireItem(XApiStatementItemViewModel item)
        {
            item.CopyToClipboard = async text =>
            {
                var top = TopLevel.GetTopLevel(this);
                if (top?.Clipboard is not null)
                    await top.Clipboard.SetTextAsync(text);
            };
        }

        foreach (var s in vm.Statements)
            WireItem(s);

        vm.Statements.CollectionChanged += (_, e) =>
        {
            if (e.NewItems is null) return;
            foreach (var o in e.NewItems)
                if (o is XApiStatementItemViewModel item)
                    WireItem(item);
        };
    }
}
