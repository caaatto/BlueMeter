using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace BlueMeter.Converters;

/// <summary>
/// Creates a horizontal-only margin (left/right) equal to half of the provided height.
/// Expected input: height (double).
/// </summary>
public sealed class HalfHeightToHorizontalMarginConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!ConverterNumberHelper.TryToDouble(value, out var height))
        {
            height = 0d;
        }

        var half = height / 2d;
        return new Thickness(half, 0, half, 0);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}
