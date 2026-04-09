using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace BlueMeter.Converters;

/// <summary>
/// Converts a hex color string or special theme names to semi-transparent SolidColorBrush for overlay effects.
/// Supports gradient themes (Rainbow, Sunset, Cyberpunk).
/// Returns null for "Transparent" theme or invalid colors.
/// </summary>
public class ThemeColorOverlayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string colorString && !string.IsNullOrEmpty(colorString))
        {
            // Check for transparent theme
            if (colorString.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            Color overlayColor;

            // Handle special gradient themes - use dominant color
            switch (colorString.ToLower())
            {
                case "rainbow":
                    overlayColor = Color.FromRgb(255, 0, 127); // Magenta
                    break;
                case "sunset":
                    overlayColor = Color.FromRgb(255, 107, 107); // Red
                    break;
                case "cyberpunk":
                    overlayColor = Color.FromRgb(255, 0, 110); // Hot Pink
                    break;
                default:
                    if (!Color.TryParse(colorString, out overlayColor))
                    {
                        return null;
                    }
                    break;
            }

            // Parse opacity from parameter, default to 0.15 (15%)
            double opacity = 0.15;
            if (parameter is string opacityString && double.TryParse(opacityString, out var parsedOpacity))
            {
                opacity = parsedOpacity;
            }

            return new SolidColorBrush(overlayColor)
            {
                Opacity = opacity
            };
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}
