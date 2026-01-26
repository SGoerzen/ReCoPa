using System;
using System.Reactive;
using ReactiveUI;

namespace ReCoPa.ViewModels;

public class SidebarViewModel : ReactiveObject
{
    private string _clientName = string.Empty;

    public string ClientName
    {
        get => _clientName;
        set => this.RaiseAndSetIfChanged(ref _clientName, value);
    }

    // Navigations-Commands
    public ReactiveCommand<Unit, Unit> NavigateToOverviewCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToActorCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToXapiCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToSettingsCommand { get; }

    public SidebarViewModel()
    {
        // Initialisiere die Commands mit leeren Aktionen (können später gefüllt werden)
        NavigateToOverviewCommand = ReactiveCommand.Create(() =>
        {
            Console.WriteLine("Navigate to Overview");
            // Hier Logik für die Navigation einfügen
        });

        NavigateToActorCommand = ReactiveCommand.Create(() =>
        {
            Console.WriteLine("Navigate to Actor");
            // Hier Logik für die Navigation einfügen
        });

        NavigateToXapiCommand = ReactiveCommand.Create(() =>
        {
            Console.WriteLine("Navigate to xAPI Statements");
            // Hier Logik für die Navigation einfügen
        });

        NavigateToSettingsCommand = ReactiveCommand.Create(() =>
        {
            Console.WriteLine("Navigate to Settings");
            // Hier Logik für die Navigation einfügen
        });
    }
}