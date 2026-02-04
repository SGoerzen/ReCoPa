using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace ReCoPa.ViewModels;

public class AnnotationViewModel
{
    public string Note { get; set; }

    public ICommand AddCommand => new RelayCommand(() =>
    {
        // optional: convert to xAPI annotation statement
        Note = string.Empty;
    });
}