using System.Collections.ObjectModel;
using ReCoPa.XApi;

namespace ReCoPa.xAPI.ViewModels;

public class FocusItem
{
    public string ObjectId { get; set; }
    public int Score { get; set; }
}

public class FocusDistributionViewModel
{
    public ObservableCollection<FocusItem> Items { get; } = new();

    public void OnXApiStatement(XApiStatement stmt)
    {
        if (!stmt.IsGaze) return;

        var item = Items.FirstOrDefault(i => i.ObjectId == stmt.ObjectId);
        if (item == null)
        {
            item = new FocusItem { ObjectId = stmt.ObjectId };
            Items.Add(item);
        }

        item.Score = Math.Min(100, item.Score + 10);
    }
}