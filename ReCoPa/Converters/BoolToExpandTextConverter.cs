using System;
using Avalonia.Data.Converters;
using System.Globalization;

namespace ReCoPa.Converters;

public sealed class BoolToExpandTextConverter : IValueConverter
{
    public static readonly BoolToExpandTextConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (value is bool b && b) ? "Collapse" : "Details";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}