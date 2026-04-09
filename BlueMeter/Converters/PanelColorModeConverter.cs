using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace BlueMeter.Converters;

/// <summary>
/// Converts PanelColorMode string ("Light" or "Dark") to appropriate panel background brush.
/// Light = White panels, Dark = Dark gray panels
/// </summary>
public class PanelColorModeConverter : IValueConverter
{
    private static readonly SolidColorBrush LightBrush = new(Color.FromArgb(230, 255, 255, 255)); // #E6FFFFFF - White panel
    private static readonly SolidColorBrush DarkBrush = new(Color.FromArgb(230, 50, 50, 50)); // #E6323232 - Dark gray panel
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
