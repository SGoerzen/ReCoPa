using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ReCoPa.ViewModels;

public partial class EndpointsDialogViewModel : ObservableObject
{
    public ObservableCollection<EndpointConfigViewModel> Endpoints { get; } = new();

    public EndpointsDialogViewModel(IEnumerable<EndpointSummaryItem> endpoints)
    {
        foreach (var endpoint in endpoints)
        {
            Endpoints.Add(new EndpointConfigViewModel(endpoint.Label));
        }

        if (Endpoints.Count == 0)
        {
            Endpoints.Add(new EndpointConfigViewModel("Local CSV File"));
        }
    }

    [RelayCommand]
    private void AddEndpoint()
    {
        Endpoints.Add(new EndpointConfigViewModel("Local CSV File"));
    }

    [RelayCommand]
    private void RemoveEndpoint(EndpointConfigViewModel endpoint)
    {
        Endpoints.Remove(endpoint);
    }
}

public partial class EndpointConfigViewModel : ObservableObject
{
    [ObservableProperty] private string name;
    [ObservableProperty] private string statementsFolder = string.Empty;
    [ObservableProperty] private bool oneFilePerComposer;
    [ObservableProperty] private string identifier = "{yyyyMMddHHmmss}";
    [ObservableProperty] private bool flatten;

    public bool HasFolderError => string.IsNullOrWhiteSpace(StatementsFolder);

    public EndpointConfigViewModel(string name)
    {
        this.name = name;
    }

    partial void OnStatementsFolderChanged(string value)
    {
        OnPropertyChanged(nameof(HasFolderError));
    }
}
