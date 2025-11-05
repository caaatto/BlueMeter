using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BlueMeter.WPF.Converters;

/// <summary>
/// Converts (opacityPercent, isEnabled) + base color (ConverterParameter) -> Color with alpha applied.
/// When isEnabled is false, returns the base color with alpha 255 (opaque).
/// Parameter can be a Color, SolidColorBrush, or string color (e.g., "#BABABA").
/// </summary>
public sealed class ConditionalPercentToColorConverter : IMultiValueConverter
{
    public object Convert(object[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        var baseColor = GetBaseColor(parameter);

        if (values is null || values.Length < 2)
            return baseColor;

        var enabled = values[1] as bool? ?? (values[1] is string s && bool.TryParse(s, out var b) && b);

        // If not enabled, force opaque
        if (!enabled)
        {
            baseColor.A = 0xFF;
            return baseColor;
        }

        var factor = GetOpacityFactor(values[0], culture);
        var scaled = Math.Clamp(Math.Round(factor * 255d), 0d, 255d);
        baseColor.A = (byte)scaled;
        return baseColor;
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        return [DependencyProperty.UnsetValue, DependencyProperty.UnsetValue];
    }

    private static Color GetBaseColor(object? parameter)
    {
        return parameter switch
        {
            Color color => color,
            SolidColorBrush brush => brush.Color,
            string colorString when ColorConverter.ConvertFromString(colorString) is Color parsedColor => parsedColor,
            _ => Colors.Transparent
        };
    }

    private static double GetOpacityFactor(object? value, CultureInfo culture)
    {
        return value switch
        {
            double d when d <= 1d => Math.Clamp(d, 0d, 1d),
            double d => Math.Clamp(d / 100d, 0d, 1d),
            int i => Math.Clamp(i / 100d, 0d, 1d),
            string s when double.TryParse(s, NumberStyles.Any, culture, out var parsed) => Math.Clamp(parsed / 100d, 0d,
                1d),
            _ => 1d
        };
    }
}