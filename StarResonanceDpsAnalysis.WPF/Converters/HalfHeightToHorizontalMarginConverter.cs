using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StarResonanceDpsAnalysis.WPF.Converters;

/// <summary>
/// Creates a horizontal-only margin (left/right) equal to half of the provided height.
/// Expected input: height (double).
/// </summary>
public sealed class HalfHeightToHorizontalMarginConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!ConverterNumberHelper.TryToDouble(value, out var height))
        {
            height = 0d;
        }

        var half = height / 2d;
        return new Thickness(half, 0, half, 0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}

