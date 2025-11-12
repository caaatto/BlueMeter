using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BlueMeter.WPF.Converters;

/// <summary>
/// Converts a hex color string (e.g., "#1690F8") or special theme names to appropriate brushes.
/// Supports both solid colors and gradient themes (Rainbow, Sunset, Cyberpunk).
/// Returns a default brush if the color string is invalid.
/// </summary>
public class ThemeColorConverter : IValueConverter
{
    private static readonly SolidColorBrush DefaultBrush = new(Color.FromRgb(0, 71, 171)); // #0047AB - Cobalt Blue

    static ThemeColorConverter()
    {
        DefaultBrush.Freeze();
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string colorString && !string.IsNullOrEmpty(colorString))
        {
            // Handle special gradient themes - use dominant/primary color
            switch (colorString.ToLower())
            {
                case "transparent":
                    // Return transparent brush for no overlay
                    return new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

                case "rainbow":
                    // Primary color for Rainbow: Magenta (average of pink and blue)
                    return new SolidColorBrush(Color.FromRgb(255, 0, 127));

                case "sunset":
                    // Primary color for Sunset: Orange
                    return new SolidColorBrush(Color.FromRgb(255, 107, 107));

                case "cyberpunk":
                    // Primary color for Cyberpunk: Magenta/Hot Pink
                    return new SolidColorBrush(Color.FromRgb(255, 0, 110));
            }

            // Try parsing as hex color
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(colorString);
                var brush = new SolidColorBrush(color);
                brush.Freeze();
                return brush;
            }
            catch
            {
                // Invalid color string, return default
                return DefaultBrush;
            }
        }

        return DefaultBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is SolidColorBrush brush)
        {
            return brush.Color.ToString();
        }

        return Binding.DoNothing;
    }
}
