using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BlueMeter.WPF.Converters;

/// <summary>
/// Converts theme color to an appropriate accent color (brighter/more saturated version).
/// Used for highlights, borders, and emphasis elements.
/// </summary>
public class ThemeAccentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string themeColor && !string.IsNullOrEmpty(themeColor))
        {
            // Get the base color and brighten it for accent use
            Color baseColor = GetThemeBaseColor(themeColor);
            Color accentColor = BrightenColor(baseColor, 1.5);

            var brush = new SolidColorBrush(accentColor);
            brush.Freeze();
            return brush;
        }

        return new SolidColorBrush(Color.FromRgb(0, 217, 255)); // Default cyan
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
        // Brighten by increasing values but capping at 255
        return Color.FromRgb(
            (byte)Math.Min(255, color.R * factor),
            (byte)Math.Min(255, color.G * factor),
            (byte)Math.Min(255, color.B * factor)
        );
    }
}
