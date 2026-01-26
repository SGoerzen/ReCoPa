using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ReCoPa.Converters;

public class BoolToBrushConverter : IValueConverter
{
    public IBrush ActiveBrush { get; set; } = Brushes.DodgerBlue;
    public IBrush InactiveBrush { get; set; } = Brushes.Transparent;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b ? ActiveBrush : InactiveBrush;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}