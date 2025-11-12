using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BlueMeter.WPF.Converters;

/// <summary>
/// Converts theme color to appropriate text/foreground color.
/// Returns lighter text for dark backgrounds and darker text for light backgrounds.
/// Also provides contrast-optimized text for readability.
/// </summary>
public class ThemeForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string themeColor && !string.IsNullOrEmpty(themeColor))
        {
            Color baseColor = GetThemeBaseColor(themeColor);

            // parameter can specify: "text" (normal text), "label" (labels), "accent" (colored text)
            string type = parameter?.ToString()?.ToLower() ?? "text";

            // Calculate brightness using relative luminance (more accurate for human perception)
            // Formula: (0.299 * R + 0.587 * G + 0.114 * B) / 255
            double brightness = (0.299 * baseColor.R + 0.587 * baseColor.G + 0.114 * baseColor.B) / 255.0;

            return type switch
            {
                "text" => brightness > 0.35
                    ? new SolidColorBrush(Color.FromRgb(0, 0, 0)) // Dark text for light backgrounds
                    : new SolidColorBrush(Color.FromRgb(255, 255, 255)), // Light text for dark backgrounds

                "label" => brightness > 0.35
                    ? new SolidColorBrush(Color.FromRgb(80, 80, 80)) // Dark gray for light backgrounds
                    : new SolidColorBrush(Color.FromRgb(255, 255, 255)), // White text for dark backgrounds

                "accent" => new SolidColorBrush(BrightenColor(baseColor, 1.2)), // Brightened theme color

                _ => new SolidColorBrush(Color.FromRgb(255, 255, 255))
            };
        }

        return new SolidColorBrush(Color.FromRgb(255, 255, 255)); // Default white
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }

    private Color GetThemeBaseColor(string themeColor)
    {
        return themeColor.ToLower() switch
        {
            "transparent" => Color.FromRgb(128, 128, 128),
            "rainbow" => Color.FromRgb(255, 0, 127),
            "sunset" => Color.FromRgb(255, 107, 107),
            "cyberpunk" => Color.FromRgb(255, 0, 110),
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
            return Color.FromRgb(22, 144, 248);
        }
    }

    private Color BrightenColor(Color color, double factor)
    {
        return Color.FromRgb(
            (byte)Math.Min(255, color.R * factor),
            (byte)Math.Min(255, color.G * factor),
            (byte)Math.Min(255, color.B * factor)
        );
    }
}
