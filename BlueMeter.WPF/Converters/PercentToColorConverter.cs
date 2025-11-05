using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BlueMeter.WPF.Converters;

public sealed class PercentToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var color = GetBaseColor(parameter);
        var factor = GetOpacityFactor(value);
        var scaled = Math.Clamp(Math.Round(factor * 255d), 0d, 255d);
        color.A = (byte)scaled;
        return color;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }

    private static Color GetBaseColor(object? parameter)
    {
        if (parameter is Color color)
        {
            return color;
        }

        if (parameter is SolidColorBrush brush)
        {
            return brush.Color;
        }

        if (parameter is string colorString && ColorConverter.ConvertFromString(colorString) is Color parsedColor)
        {
            return parsedColor;
        }

        return Colors.Transparent;
    }

    private static double GetOpacityFactor(object? value)
    {
        return value switch
        {
            double doubleValue when doubleValue <= 1d => Math.Clamp(doubleValue, 0d, 1d),
            double doubleValue => Math.Clamp(doubleValue / 100d, 0d, 1d),
            int intValue => Math.Clamp(intValue / 100d, 0d, 1d),
            string stringValue when double.TryParse(stringValue, out var parsed) => Math.Clamp(parsed / 100d, 0d, 1d),
            _ => 1d
        };
    }
}