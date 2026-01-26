using System;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using ReCoPa.Models;

namespace ReCoPa.ViewModels;

public class StatementListViewModel : ReactiveObject
{
    public ObservableCollection<IStatement> Statements { get; } = [];
    public ObservableCollection<string> FilterOptions { get; } = ["All Statements", "Recent"];
    public ReactiveCommand<Unit, Unit> ExportCsvCommand { get; }
    public ReactiveCommand<Unit, Unit> StartCalibrationCommand { get; }
    public ReactiveCommand<Unit, Unit> ShutdownAppCommand { get; }
    
    private string _selectedFilter = "All Statements";
    public string SelectedFilter
    {
        get => _selectedFilter;
        set => this.RaiseAndSetIfChanged(ref _selectedFilter, value);
    }

    public StatementListViewModel()
    {
        ExportCsvCommand = ReactiveCommand.Create(() => { /* Logik für CSV-Export */ });
        StartCalibrationCommand = ReactiveCommand.Create(() => { /* Logik für Eye Calibration */ });
        ShutdownAppCommand = ReactiveCommand.Create(() => { /* Logik für Shutdown */ });
    }
}