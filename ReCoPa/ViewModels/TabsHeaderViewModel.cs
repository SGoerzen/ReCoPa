using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class TabsHeaderViewModel : INotifyPropertyChanged
{
    double _offsetX, _extentW, _viewportW;

    public bool CanScrollLeft  => _offsetX > 0.5;
    public bool CanScrollRight => _offsetX + _viewportW < _extentW - 0.5;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void UpdateScroll(double offsetX, double extentWidth, double viewportWidth)
    {
        _offsetX = offsetX;
        _extentW = extentWidth;
        _viewportW = viewportWidth;

        OnPropertyChanged(nameof(CanScrollLeft));
        OnPropertyChanged(nameof(CanScrollRight));
    }

    void OnPropertyChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}