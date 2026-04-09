using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace BlueMeter.Converters;

/// <summary>
/// Converts PanelColorMode string ("Light" or "Dark") to appropriate panel content background brush.
/// Used for the main content area of panels.
/// Light = Lighter background, Dark = Darker background
/// </summary>
public class PanelContentColorModeConverter : IValueConverter
{
    private static readonly SolidColorBrush LightBrush = new(Color.FromArgb(220, 245, 245, 245)); // #DCF5F5F5 - Light gray content
    private static readonly SolidColorBrush DarkBrush = new(Color.FromArgb(220, 40, 40, 40)); // #DC282828 - Dark content
    private static readonly SolidColorBrush DefaultBrush = DarkBrush;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string mode)
        {
            return mode.Equals("Light", StringComparison.OrdinalIgnoreCase) ? LightBrush : DarkBrush;
        }

        return DefaultBrush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return BindingOperations.DoNothing;
    }
}
