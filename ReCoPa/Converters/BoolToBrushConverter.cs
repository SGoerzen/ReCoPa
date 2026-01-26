using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ReCoPa.Converters;

public class BoolToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isActive = value is bool b && b;

        // resource keys you control in Colors.axaml
        var active = (IBrush?)Application.Current?.FindResource("TabActiveBackground");
        var inactive = (IBrush?)Application.Current?.FindResource("TabInactiveBackground");

        return isActive ? (active ?? Brushes.Transparent) : (inactive ?? Brushes.Transparent);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}