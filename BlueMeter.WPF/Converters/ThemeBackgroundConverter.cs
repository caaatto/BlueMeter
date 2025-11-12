using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BlueMeter.WPF.Converters;

/// <summary>
/// Converts theme color to appropriate background brush for UI elements.
/// Provides dark, medium, and light background shades based on the selected theme.
/// </summary>
public class ThemeBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string themeColor && !string.IsNullOrEmpty(themeColor))
        {
            // Get the base color
            Color baseColor = GetThemeBaseColor(themeColor);

            // parameter determines which shade: "dark", "medium", or "light"
            string shade = parameter?.ToString()?.ToLower() ?? "medium";

            return shade switch
            {
                "dark" => new SolidColorBrush(DarkenColor(baseColor, 0.7)), // Very dark background
                "medium" => new SolidColorBrush(DarkenColor(baseColor, 0.5)), // Medium background
                "light" => new SolidColorBrush(DarkenColor(baseColor, 0.3)), // Lighter background
                _ => new SolidColorBrush(DarkenColor(baseColor, 0.5))
            };
        }

        return new SolidColorBrush(Color.FromRgb(18, 25, 43)); // Default dark blue
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }

    private Color GetThemeBaseColor(string themeColor)
    {
        return themeColor.ToLower() switch
        {
            "transparent" => Color.FromRgb(128, 128, 128), // Gray for transparent theme
            "rainbow" => Color.FromRgb(255, 0, 127), // Magenta
            "sunset" => Color.FromRgb(255, 107, 107), // Red
            "cyberpunk" => Color.FromRgb(255, 0, 110), // Hot Pink
            _ => ParseColorFromString(themeColor)
        };
    }

    private Color ParseColorFromString(string colorString)
    {
        try
        {
            return (Color)ColorConverter.ConvertFromString(colorString);
        }
        catch
        {
            return Color.FromRgb(22, 144, 248); // Default blue
        }
    }

    private Color DarkenColor(Color color, double factor)
    {
        // Darken by reducing brightness
        return Color.FromRgb(
            (byte)(color.R * factor),
            (byte)(color.G * factor),
            (byte)(color.B * factor)
        );
    }
}
