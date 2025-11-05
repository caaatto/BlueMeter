using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BlueMeter.WPF.Converters;

/// <summary>
/// Converts a hex color string (e.g., "#1690F8") to a SolidColorBrush for theme customization.
/// Returns a default brush if the color string is invalid.
/// </summary>
public class ThemeColorConverter : IValueConverter
{
    private static readonly SolidColorBrush DefaultBrush = new(Color.FromRgb(22, 144, 248)); // #1690F8

    static ThemeColorConverter()
    {
        DefaultBrush.Freeze();
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string colorString && !string.IsNullOrEmpty(colorString))
        {
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
