using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace BlueMeter.Converters;

/// <summary>
/// Converts a hex color string (e.g., "#1690F8") or special theme names to appropriate brushes.
/// Supports both solid colors and gradient themes (Rainbow, Sunset, Cyberpunk).
/// Returns a default brush if the color string is invalid.
/// </summary>
public class ThemeColorConverter : IValueConverter
{
    private static readonly SolidColorBrush DefaultBrush = new(Color.FromRgb(0, 71, 171)); // #0047AB - Cobalt Blue

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
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
            if (Color.TryParse(colorString, out var color))
            {
                return new SolidColorBrush(color);
            }

            return DefaultBrush;
        }

        return DefaultBrush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ISolidColorBrush brush)
        {
            return brush.Color.ToString();
        }

        return BindingOperations.DoNothing;
    }
}
