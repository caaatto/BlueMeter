using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BlueMeter.WPF.Converters;

/// <summary>
/// Converts a hex color string to a semi-transparent SolidColorBrush for overlay effects.
/// Returns null for "Transparent" theme or invalid colors.
/// </summary>
public class ThemeColorOverlayConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string colorString && !string.IsNullOrEmpty(colorString))
        {
            // Check for transparent theme
            if (colorString.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            try
            {
                var color = (Color)ColorConverter.ConvertFromString(colorString);

                // Parse opacity from parameter, default to 0.15 (15%)
                double opacity = 0.15;
                if (parameter is string opacityString && double.TryParse(opacityString, out var parsedOpacity))
                {
                    opacity = parsedOpacity;
                }

                var brush = new SolidColorBrush(color)
                {
                    Opacity = opacity
                };
                brush.Freeze();
                return brush;
            }
            catch
            {
                // Invalid color string, return null
                return null;
            }
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
