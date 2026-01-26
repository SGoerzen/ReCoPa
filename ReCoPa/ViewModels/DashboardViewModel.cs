

using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;

namespace ReCoPa.ViewModels;

public class DashboardViewModel : ReactiveObject
{
    public ObservableCollection<TabViewModel> ClientTabs { get; } = [];
    public ReactiveCommand<Unit, Unit> AddClientCommand { get; }

    private string _selectedClientName = "VR Experience - PC2";
    public string SelectedClientName
    {
        get => _selectedClientName;
        set => this.RaiseAndSetIfChanged(ref _selectedClientName, value);
    }

    public DashboardViewModel()
    {
        // Beispiel-Tabs hinzufügen
        ClientTabs.Add(new TabViewModel { Header = "VR Training - PC1", IsActive = true });
        ClientTabs.Add(new TabViewModel { Header = "Cognitive Test - Lab-PC", IsActive = false });
        ClientTabs.Add(new TabViewModel { Header = "VR Experience - PC2", IsActive = true });

        AddClientCommand = ReactiveCommand.Create(() =>
        {
            ClientTabs.Add(new TabViewModel { Header = "New Client", IsActive = false });
        });
    }
}
