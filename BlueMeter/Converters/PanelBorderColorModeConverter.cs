using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace BlueMeter.Converters;

/// <summary>
/// Converts PanelColorMode string ("Light" or "Dark") to appropriate border color.
/// Light mode = Dark borders, Dark mode = Light borders
/// </summary>
public class PanelBorderColorModeConverter : IValueConverter
{
    private static readonly SolidColorBrush LightModeBrush = new(Color.FromRgb(200, 200, 200)); // #C8C8C8 - Dark gray border for light mode
    private static readonly SolidColorBrush DarkModeBrush = new(Color.FromRgb(100, 100, 100)); // #646464 - Light gray border for dark mode
    private static readonly SolidColorBrush DefaultBrush = DarkModeBrush;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string mode)
        {
            return mode.Equals("Light", StringComparison.OrdinalIgnoreCase) ? LightModeBrush : DarkModeBrush;
        }

        return DefaultBrush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}
